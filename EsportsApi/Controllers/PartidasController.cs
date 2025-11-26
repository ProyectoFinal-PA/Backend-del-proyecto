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
        private readonly HttpClient _httpClient;

        public PartidasController(ApplicationDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient(); 
        }

        // GET: Ver partidas (Incluye datos del Bracket)
        [HttpGet("torneo/{tournamentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPartidasForTournament(int tournamentId)
        {
            var partidas = await _context.Partidas
                .Where(p => p.TournamentId == tournamentId)
                .Include(p => p.TeamA)
                .Include(p => p.TeamB)
                .Include(p => p.Resultado)
                .OrderBy(p => p.Round) // Ordenar por ronda
                .ThenBy(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.ScheduledTime,
                    p.Status,
                    p.Round,      
                    p.Label,      
                    p.NextMatchId, 
                    TeamA = p.TeamA != null ? p.TeamA.Name : "TBD", // TBD = A determinar
                    TeamB = p.TeamB != null ? p.TeamB.Name : "TBD",
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

        // POST: Crear Partida Manualmente
        [HttpPost]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> CreatePartida([FromBody] CreatePartidaDto dto)
        {
            var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);
            if (tournament == null) return NotFound("Torneo no encontrado");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (tournament.OrganizadorId != userId && userRole != "Admin")
                return Forbid("No eres el organizador de este torneo.");
            
            var partida = new Partida
            {
                TournamentId = dto.TournamentId,
                TeamA_Id = dto.TeamA_Id,
                TeamB_Id = dto.TeamB_Id,
                ScheduledTime = dto.ScheduledTime,
                Status = "Pendiente",
                TwitchChannelName = dto.TwitchChannelName,
                Round = 1, // Por defecto ronda 1 si es manual
                Label = "Partida Manual"
            };

            _context.Partidas.Add(partida);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Partida creada", partidaId = partida.Id });
        }

        // POST: GENERAR BRACKET AUTOMÁTICO (Lógica de Árbol)
        [HttpPost("generar/{tournamentId}")]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> GenerateFixture(int tournamentId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (tournament == null || (tournament.OrganizadorId != userId && userRole != "Admin"))
                return Forbid();

            if (await _context.Partidas.AnyAsync(p => p.TournamentId == tournamentId))
                return BadRequest(new { message = "El bracket ya existe." });

            var teams = await _context.Teams.Where(t => t.TournamentId == tournamentId).ToListAsync();
            
            int count = teams.Count;
            // Validamos potencia de 2 básica (2, 4, 8, 16)
            if (count < 2 || (count & (count - 1)) != 0)
                return BadRequest(new { message = "Necesitas 2, 4, 8 o 16 equipos para un bracket perfecto." });

            // Barajar
            var random = new Random();
            var shuffledTeams = teams.OrderBy(x => random.Next()).ToList();

            List<Partida> currentRoundMatches = new List<Partida>();
            int roundNumber = 1;

            // 1. Crear Ronda 1
            for (int i = 0; i < shuffledTeams.Count; i += 2)
            {
                var partida = new Partida
                {
                    TournamentId = tournamentId,
                    TeamA_Id = shuffledTeams[i].Id,
                    TeamB_Id = shuffledTeams[i + 1].Id,
                    ScheduledTime = DateTime.Now.AddDays(1),
                    Status = "Pendiente",
                    TwitchChannelName = tournament.KickChannel,
                    Round = roundNumber,
                    Label = $"Ronda {roundNumber}"
                };
                currentRoundMatches.Add(partida);
                _context.Partidas.Add(partida);
            }
            await _context.SaveChangesAsync(); 

            // 2. Crear Rondas Siguientes (Vacías)
            while (currentRoundMatches.Count > 1)
            {
                roundNumber++;
                List<Partida> nextRoundMatches = new List<Partida>();

                for (int i = 0; i < currentRoundMatches.Count; i += 2)
                {
                    var nextMatch = new Partida
                    {
                        TournamentId = tournamentId,
                        TeamA_Id = null, // TBD
                        TeamB_Id = null, // TBD
                        ScheduledTime = DateTime.Now.AddDays(roundNumber),
                        Status = "Pendiente",
                        TwitchChannelName = tournament.KickChannel,
                        Round = roundNumber,
                        Label = (currentRoundMatches.Count == 2) ? "GRAN FINAL" : $"Ronda {roundNumber}"
                    };
                    _context.Partidas.Add(nextMatch);
                    await _context.SaveChangesAsync(); 
                    
                    nextRoundMatches.Add(nextMatch);

                    // Conectar las partidas previas a esta nueva
                    currentRoundMatches[i].NextMatchId = nextMatch.Id;
                    currentRoundMatches[i+1].NextMatchId = nextMatch.Id;
                }
                await _context.SaveChangesAsync();
                currentRoundMatches = nextRoundMatches;
            }

            return Ok(new { message = "Bracket generado exitosamente." });
        }

        // POST: Registrar Resultado y AVANZAR GANADOR
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
                return Forbid();

            var resultado = new Resultado
            {
                PartidaId = partidaId,
                ScoreTeamA = dto.ScoreTeamA,
                ScoreTeamB = dto.ScoreTeamB,
                WinnerTeamId = dto.WinnerTeamId
            };
            _context.Resultados.Add(resultado);
            partida.Status = "Jugada";

            // --- LÓGICA DE AVANCE EN EL BRACKET ---
            if (partida.NextMatchId != null)
            {
                var nextMatch = await _context.Partidas.FindAsync(partida.NextMatchId);
                if (nextMatch != null)
                {
                    if (nextMatch.TeamA_Id == null)
                        nextMatch.TeamA_Id = dto.WinnerTeamId;
                    else
                        nextMatch.TeamB_Id = dto.WinnerTeamId;
                }
            }
            // --------------------------------------

            await _context.SaveChangesAsync();
            return Ok(new { message = "Resultado registrado y ganador avanzado." });
        }
        
        // GET: Verificar Stream (¡¡ESTE ES EL QUE FALTABA!!)
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
                
                // TUS LLAVES DE TWITCH
                request.Headers.Add("Client-ID", "TU_CLIENT_ID_DE_TWITCH"); 
                request.Headers.Add("Authorization", "Bearer TU_APP_ACCESS_TOKEN"); 

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return Ok(new { isLive = false, message = "Error de API de Twitch", error = errorBody });
                }

                var content = await response.Content.ReadAsStringAsync();
                var twitchResponse = JsonDocument.Parse(content);
                var data = twitchResponse.RootElement.GetProperty("data");

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