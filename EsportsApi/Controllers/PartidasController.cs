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

        // GET: Ver partidas (Incluimos los nuevos datos)
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
                    p.Round,      // Nuevo
                    p.Label,      // Nuevo
                    p.NextMatchId, // Nuevo
                    TeamA = p.TeamA != null ? p.TeamA.Name : "TBD", // Si no hay equipo, mostrar TBD
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

        // GENERAR BRACKET AUTOMÁTICO (Algoritmo Complejo)
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
            
            // Validamos potencia de 2 (2, 4, 8, 16) para bracket perfecto
            int count = teams.Count;
            if (count < 2 || (count & (count - 1)) != 0)
                return BadRequest(new { message = "Por ahora, necesitamos 2, 4, 8 o 16 equipos para un bracket perfecto." });

            // Barajar equipos
            var random = new Random();
            var shuffledTeams = teams.OrderBy(x => random.Next()).ToList();

            // --- ALGORITMO DE CREACIÓN DE BRACKET ---
            List<Partida> currentRoundMatches = new List<Partida>();
            int roundNumber = 1;

            // 1. Crear la Ronda 1 (Con los equipos reales)
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
                    Label = $"Ronda {roundNumber} - Juego {(i/2)+1}"
                };
                currentRoundMatches.Add(partida);
                _context.Partidas.Add(partida);
            }
            await _context.SaveChangesAsync(); // Guardamos para tener IDs

            // 2. Crear Rondas Siguientes (Vacías) hasta la final
            while (currentRoundMatches.Count > 1)
            {
                roundNumber++;
                List<Partida> nextRoundMatches = new List<Partida>();

                for (int i = 0; i < currentRoundMatches.Count; i += 2)
                {
                    // Creamos la partida "padre" (vacía)
                    var nextMatch = new Partida
                    {
                        TournamentId = tournamentId,
                        TeamA_Id = null, // Esperando ganador
                        TeamB_Id = null, // Esperando ganador
                        ScheduledTime = DateTime.Now.AddDays(roundNumber),
                        Status = "Pendiente",
                        TwitchChannelName = tournament.KickChannel,
                        Round = roundNumber,
                        Label = (currentRoundMatches.Count == 2) ? "GRAN FINAL" : $"Ronda {roundNumber}"
                    };
                    _context.Partidas.Add(nextMatch);
                    await _context.SaveChangesAsync(); // Guardamos para obtener ID
                    
                    nextRoundMatches.Add(nextMatch);

                    // Conectamos los dos partidos anteriores a este nuevo
                    currentRoundMatches[i].NextMatchId = nextMatch.Id;
                    currentRoundMatches[i+1].NextMatchId = nextMatch.Id;
                }
                
                // Guardamos las conexiones
                await _context.SaveChangesAsync();
                
                // Avanzamos a la siguiente ronda
                currentRoundMatches = nextRoundMatches;
            }

            return Ok(new { message = "Bracket generado exitosamente." });
        }

        // REGISTRAR RESULTADO Y AVANZAR GANADOR
        [HttpPost("{partidaId}/resultado")]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> RegisterResultado(int partidaId, [FromBody] RegisterResultadoDto dto)
        {
            var partida = await _context.Partidas
                .Include(p => p.Tournament)
                .FirstOrDefaultAsync(p => p.Id == partidaId);

            if (partida == null) return NotFound("Partida no encontrada");

            // Validar permisos...
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (partida.Tournament.OrganizadorId != userId && userRole != "Admin")
                return Forbid();

            // Guardar resultado
            var resultado = new Resultado
            {
                PartidaId = partidaId,
                ScoreTeamA = dto.ScoreTeamA,
                ScoreTeamB = dto.ScoreTeamB,
                WinnerTeamId = dto.WinnerTeamId
            };
            _context.Resultados.Add(resultado);
            partida.Status = "Jugada";

            // --- LÓGICA DE AVANCE DE RONDA ---
            if (partida.NextMatchId != null)
            {
                var nextMatch = await _context.Partidas.FindAsync(partida.NextMatchId);
                if (nextMatch != null)
                {
                    // Si el slot A está vacío, ponlo ahí. Si no, ponlo en el B.
                    if (nextMatch.TeamA_Id == null)
                        nextMatch.TeamA_Id = dto.WinnerTeamId;
                    else
                        nextMatch.TeamB_Id = dto.WinnerTeamId;
                }
            }
            // ---------------------------------

            await _context.SaveChangesAsync();
            return Ok(new { message = "Resultado registrado y ganador avanzado." });
        }

        // (Mantén aquí el método GetLiveStatus tal cual estaba)
        [HttpGet("{partidaId}/live")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLiveStatus(int partidaId)
        {
             // ... (Pega aquí el código de GetLiveStatus que ya tenías) ...
             return Ok(new { isLive = false }); // Placeholder si no lo pegas
        }
    }
}