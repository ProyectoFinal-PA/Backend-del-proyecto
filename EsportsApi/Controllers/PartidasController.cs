// En Controllers/PartidasController.cs
using EsportsApi.Data;
using EsportsApi.DTOs;
using EsportsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EsportsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PartidasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. POST (Crear una Partida) ---
        // Solo el Organizador del torneo (o un Admin) pueden crear partidas
        [HttpPost]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> CreatePartida([FromBody] CreatePartidaDto dto)
        {
            var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);
            if (tournament == null) return NotFound("Torneo no encontrado");

            // --- Lógica de Permisos ---
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            // Si NO es el dueño del torneo Y TAMPOCO es Admin
            if (tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid("No eres el organizador de este torneo.");
            }
            // ------------------------

            var partida = new Partida
            {
                TournamentId = dto.TournamentId,
                TeamA_Id = dto.TeamA_Id,
                TeamB_Id = dto.TeamB_Id,
                ScheduledTime = dto.ScheduledTime,
                Status = "Pendiente"
            };

            _context.Partidas.Add(partida);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Partida creada", partidaId = partida.Id });
        }

        // --- 2. GET (Ver partidas de un torneo) ---
        // Cualquiera puede ver las partidas
        [HttpGet("torneo/{tournamentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPartidasForTournament(int tournamentId)
        {
            var partidas = await _context.Partidas
                .Where(p => p.TournamentId == tournamentId)
                .Include(p => p.TeamA) // Traer nombre Equipo A
                .Include(p => p.TeamB) // Traer nombre Equipo B
                .Include(p => p.Resultado) // Traer el resultado
                .Select(p => new
                {
                    p.Id,
                    p.ScheduledTime,
                    p.Status,
                    TeamA = p.TeamA.Name,
                    TeamB = p.TeamB.Name,
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

        // --- 3. POST (Registrar un Resultado) ---
        // Solo el Organizador del torneo (o un Admin) pueden registrar resultados
        [HttpPost("{partidaId}/resultado")]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> RegisterResultado(int partidaId, [FromBody] RegisterResultadoDto dto)
        {
            var partida = await _context.Partidas
                .Include(p => p.Tournament) // Cargar el Torneo para verificar al dueño
                .FirstOrDefaultAsync(p => p.Id == partidaId);

            if (partida == null) return NotFound("Partida no encontrada");

            // --- Lógica de Permisos ---
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            // Si NO es el dueño del torneo de la partida Y TAMPOCO es Admin
            if (partida.Tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid("No puedes registrar resultados para este torneo.");
            }
            // ------------------------

            var resultado = new Resultado
            {
                PartidaId = partidaId,
                ScoreTeamA = dto.ScoreTeamA,
                ScoreTeamB = dto.ScoreTeamB,
                WinnerTeamId = dto.WinnerTeamId
            };

            _context.Resultados.Add(resultado);
            partida.Status = "Jugada"; // Actualizamos el estado de la partida
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resultado registrado" });
        }
    }
}