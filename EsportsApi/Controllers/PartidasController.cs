using EsportsApi.Data;
using EsportsApi.DTOs;
using EsportsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EsportsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //se usa para llamar a APIs externas
        private readonly HttpClient _httpClient;

        public PartidasController(ApplicationDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient(); 
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> CreatePartida([FromBody] CreatePartidaDto dto)
        {
            var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);
            if (tournament == null) return NotFound("Torneo no encontrado");

            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid("No eres el organizador de este torneo.");
            }
            
            var partida = new Partida
            {
                TournamentId = dto.TournamentId,
                TeamA_Id = dto.TeamA_Id,
                TeamB_Id = dto.TeamB_Id,
                ScheduledTime = dto.ScheduledTime,
                Status = "Pendiente",
                TwitchChannelName = dto.TwitchChannelName 
            };

            _context.Partidas.Add(partida);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Partida creada", partidaId = partida.Id });
        }

       
        [HttpGet("torneo/{tournamentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPartidasForTournament(int tournamentId)
        {
            var partidas = await _context.Partidas
                .Where(p => p.TournamentId == tournamentId)
                .Include(p => p.TeamA)
                .Include(p => p.TeamB)
                .Include(p => p.Resultado)
                .Select(p => new
                {
                    p.Id,
                    p.ScheduledTime,
                    p.Status,
                    TeamA = p.TeamA.Name,
                    TeamB = p.TeamB.Name,
                    TwitchChannel = p.TwitchChannelName, 
                    Resultado = p.Resultado == null ? null : new 
                    {
                        p.Resultado.ScoreTeamA,
                        p.Resultado.ScoreTeamB,
                        p.Resultado.WinnerTeamId
                    }
                })
                .ToListAsync();

            return Ok(partidas);
        }

        
        [HttpPost("{partidaId}/resultado")]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> RegisterResultado(int partidaId, [FromBody] RegisterResultadoDto dto)
        {
            var partida = await _context.Partidas
                .Include(p => p.Tournament) 
                .FirstOrDefaultAsync(p => p.Id == partidaId);

            if (partida == null) return NotFound("Partida no encontrada");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (partida.Tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid("No puedes registrar resultados para este torneo.");
            }
            
            var resultado = new Resultado
            {
                PartidaId = partidaId,
                ScoreTeamA = dto.ScoreTeamA,
                ScoreTeamB = dto.ScoreTeamB,
                WinnerTeamId = dto.WinnerTeamId
            };

            _context.Resultados.Add(resultado);
            partida.Status = "Jugada"; 
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resultado registrado" });
        }
        
        // GET (Verificar si está en vivo) 
        [HttpGet("{partidaId}/live")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLiveStatus(int partidaId)
        {
            var partida = await _context.Partidas.FindAsync(partidaId);
            
            if (partida == null || string.IsNullOrEmpty(partida.TwitchChannelName))
            {
                return Ok(new { isLive = false, message = "Partida no encontrada o sin canal." });
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://api.twitch.tv/helix/streams?user_login={partida.TwitchChannelName}");
                
                
                // (Ahora mismo fallará, porque necesitamos las llaves reales)
                request.Headers.Add("Client-ID", "TU_CLIENT_ID_DE_TWITCH"); //NECESITAMOS CAMBIAR ESTO
                request.Headers.Add("Authorization", "Bearer TU_APP_ACCESS_TOKEN"); //Y ESTO

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    // Si Twitch falla (ej. llaves incorrectas), asumimos offline
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return Ok(new { isLive = false, message = "Error de API de Twitch", error = errorBody });
                }

                var content = await response.Content.ReadAsStringAsync();
                var twitchResponse = JsonDocument.Parse(content);
                var data = twitchResponse.RootElement.GetProperty("data");

                // Si "data" es un array con al menos 1 elemento, ¡está en vivo!
                if (data.GetArrayLength() > 0)
                {
                    return Ok(new { isLive = true, channel = partida.TwitchChannelName });
                }
                
                return Ok(new { isLive = false, message = "Offline" });
            }
            catch (Exception ex)
            {
                return Ok(new { isLive = false, message = ex.Message });
            }
        }
    }
}