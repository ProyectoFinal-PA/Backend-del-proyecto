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
    [Authorize] // Todos los endpoints aquí requieren estar logueado (excepto los que digan [AllowAnonymous])
    public class EquiposController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EquiposController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. POST (Crear un equipo) ---
        // Un Jugador crea un equipo y se vuelve Capitán
        [HttpPost]
        [Authorize(Roles = "Jugador")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Verificar que el torneo exista
            var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);
            if (tournament == null)
            {
                return BadRequest("El torneo no existe.");
            }
            
            // Verificar que el usuario no esté ya en un equipo
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user.TeamId != null)
            {
                return BadRequest("Ya perteneces a un equipo.");
            }

            var newTeam = new Team
            {
                Name = dto.Name,
                TournamentId = dto.TournamentId,
                CaptainId = userId // El creador es el Capitán
            };

            // Añadir al capitán como el primer miembro
            newTeam.Members.Add(user);
            user.TeamId = newTeam.Id; // Asignar el TeamId al usuario

            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Equipo creado con éxito", teamId = newTeam.Id });
        }

        // --- 2. POST (Unirse a un equipo) ---
        // Un Jugador se une a un equipo existente
        [HttpPost("{teamId}/join")]
        [Authorize(Roles = "Jugador")]
        public async Task<IActionResult> JoinTeam(int teamId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                return NotFound("El equipo no existe.");
            }
            
            var user = await _context.Users.FindAsync(userId);
            if (user.TeamId != null)
            {
                return BadRequest("Ya perteneces a un equipo.");
            }
            
            // (Aquí podrías añadir lógica de límite de miembros, ej: if (team.Members.Count >= 5) ...)

            team.Members.Add(user);
            user.TeamId = team.Id;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Te has unido al equipo " + team.Name });
        }

        // --- 3. GET (Ver equipos de un torneo) ---
        [HttpGet("torneo/{tournamentId}")]
        [AllowAnonymous] // Cualquiera puede ver los equipos
        public async Task<IActionResult> GetTeamsForTournament(int tournamentId)
        {
            var teams = await _context.Teams
                .Where(t => t.TournamentId == tournamentId)
                .Include(t => t.Captain) // Traer datos del Capitán
                .Include(t => t.Members) // Traer datos de Miembros
                .Include(t => t.Tournament) // Traer datos del Torneo
                .Select(t => new TeamResponseDto( // Usar el DTO para la respuesta
                    t.Id,
                    t.Name,
                    t.TournamentId,
                    t.Tournament.Name,
                    t.CaptainId,
                    t.Captain.Nickname,
                    t.Members.Select(m => m.Nickname).ToList() // Lista de apodos
                ))
                .ToListAsync();

            return Ok(teams);
        }

        // --- 4. DELETE (Borrar un equipo) ---
        // Solo el Capitán o un Admin pueden borrar el equipo
        [HttpDelete("{teamId}")]
        [Authorize(Roles = "Admin, Jugador")] // Dejamos entrar a ambos (validamos al Jugador)
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Members) // Cargar los miembros
                .FirstOrDefaultAsync(t => t.Id == teamId);
            
            if (team == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // --- Lógica de Permisos ---
            // Si el usuario NO es el capitán Y TAMPOCO es un Admin...
            if (team.CaptainId != userId && userRole != "Admin")
            {
                return Forbid(); // Error 403
            }

            // Quitar el TeamId de todos los miembros para que queden "libres"
            foreach (var member in team.Members)
            {
                member.TeamId = null;
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent(); // Éxito
        }
    }
}