// En Controllers/EquiposController.cs
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
    [Authorize] 
    public class EquiposController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EquiposController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Crear un equipo (Jugador)
        [HttpPost]
        [Authorize(Roles = "Jugador")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);
            if (tournament == null) return NotFound(new { message = "El torneo no existe." });
            
            // Verificar si ya está en un equipo de este torneo
            var isAlreadyInTeamForThisTournament = await _context.Teams
                .Include(t => t.Members)
                .AnyAsync(t => t.TournamentId == dto.TournamentId && 
                               t.Members.Any(m => m.Id == userId));

            if (isAlreadyInTeamForThisTournament)
                return BadRequest(new { message = "Ya perteneces a un equipo en este torneo." });

            var user = await _context.Users.FindAsync(userId);
            var newTeam = new Team
            {
                Name = dto.Name,
                TournamentId = dto.TournamentId,
                CaptainId = userId 
            };

            newTeam.Members.Add(user);
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();
            
            user.TeamId = newTeam.Id;
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Equipo creado con éxito", teamId = newTeam.Id });
        }

        // POST: Unirse a un equipo (Jugador)
        [HttpPost("{teamId}/join")]
        [Authorize(Roles = "Jugador")]
        public async Task<IActionResult> JoinTeam(int teamId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound(new { message = "El equipo no existe." });
            
            var isAlreadyInTeamForThisTournament = await _context.Teams
                .Include(t => t.Members)
                .AnyAsync(t => t.TournamentId == team.TournamentId && 
                               t.Members.Any(m => m.Id == userId));

            if (isAlreadyInTeamForThisTournament)
                return BadRequest(new { message = "Ya perteneces a un equipo en este torneo." });
            
            var user = await _context.Users.FindAsync(userId);
            team.Members.Add(user);
            user.TeamId = team.Id; 
            await _context.SaveChangesAsync();

            return Ok(new { message = "Te has unido al equipo " + team.Name });
        }

        // GET: Ver equipos de un torneo (Público)
        [HttpGet("torneo/{tournamentId}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetTeamsForTournament(int tournamentId)
        {
            var teams = await _context.Teams
                .Where(t => t.TournamentId == tournamentId)
                .Include(t => t.Captain) 
                .Include(t => t.Members) 
                .Include(t => t.Tournament) 
                .Select(t => new TeamResponseDto( 
                    t.Id,
                    t.Name,
                    t.TournamentId,
                    t.Tournament.Name,
                    t.CaptainId,
                    t.Captain.Nickname,
                    t.Members.Select(m => m.Nickname).ToList() 
                ))
                .ToListAsync();

            return Ok(teams);
        }

        // --- 4. DELETE (Borrar un equipo) - ¡¡MODIFICADO!! ---
        // Ahora permitimos Organizador, Admin y Jugador (Capitán)
        [HttpDelete("{teamId}")]
        [Authorize(Roles = "Admin, Organizador, Jugador")] 
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Members) 
                .Include(t => t.Tournament) // <-- Incluimos el torneo para ver quién es el dueño
                .FirstOrDefaultAsync(t => t.Id == teamId);
            
            if (team == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // --- Lógica de Permisos Mejorada ---
            bool isCaptain = team.CaptainId == userId;
            bool isAdmin = userRole == "Admin";
            bool isTournamentOrganizer = team.Tournament.OrganizadorId == userId; // <-- Chequeo nuevo

            // Si NO es capitán, NI Admin, NI el Organizador del torneo... Fuera.
            if (!isCaptain && !isAdmin && !isTournamentOrganizer)
            {
                return Forbid();
            }
            // -----------------------------------

            foreach (var member in team.Members)
            {
                member.TeamId = null;
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}