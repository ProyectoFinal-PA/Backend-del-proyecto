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

        // 1. POST: Crear equipo (CON LOGO)
        [HttpPost]
        [Authorize(Roles = "Jugador")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);
            if (tournament == null) return NotFound(new { message = "El torneo no existe." });
            
            var isAlreadyInTeam = await _context.Teams
                .Include(t => t.Members)
                .AnyAsync(t => t.TournamentId == dto.TournamentId && t.Members.Any(m => m.Id == userId));

            if (isAlreadyInTeam) return BadRequest(new { message = "Ya estás en un equipo." });

            var user = await _context.Users.FindAsync(userId);
            var newTeam = new Team
            {
                Name = dto.Name,
                LogoUrl = dto.LogoUrl, // <-- GUARDAMOS EL LOGO
                TournamentId = dto.TournamentId,
                CaptainId = userId 
            };

            newTeam.Members.Add(user);
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();
            
            user.TeamId = newTeam.Id;
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Equipo creado", teamId = newTeam.Id });
        }

        // 2. POST: Unirse (Igual que antes)
        [HttpPost("{teamId}/join")]
        [Authorize(Roles = "Jugador")]
        public async Task<IActionResult> JoinTeam(int teamId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound(new { message = "Equipo no existe." });
            
            var isAlreadyInTeam = await _context.Teams
                .Include(t => t.Members)
                .AnyAsync(t => t.TournamentId == team.TournamentId && t.Members.Any(m => m.Id == userId));

            if (isAlreadyInTeam) return BadRequest(new { message = "Ya estás en un equipo." });
            
            var user = await _context.Users.FindAsync(userId);
            team.Members.Add(user);
            user.TeamId = team.Id; 
            await _context.SaveChangesAsync();

            return Ok(new { message = "Te uniste al equipo." });
        }

        // 3. GET: Ver equipos (CON LOGO)
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
                    t.LogoUrl, // <-- DEVOLVEMOS EL LOGO
                    t.TournamentId,
                    t.Tournament.Name,
                    t.CaptainId,
                    t.Captain.Nickname,
                    t.Members.Select(m => m.Nickname).ToList() 
                ))
                .ToListAsync();

            return Ok(teams);
        }

        // 4. DELETE (Igual que antes)
        [HttpDelete("{teamId}")]
        [Authorize(Roles = "Admin, Organizador, Jugador")] 
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            var team = await _context.Teams.Include(t => t.Members).Include(t => t.Tournament).FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            bool isCaptain = team.CaptainId == userId;
            bool isAdmin = userRole == "Admin";
            bool isOrganizer = team.Tournament.OrganizadorId == userId;

            if (!isCaptain && !isAdmin && !isOrganizer) return Forbid();

            foreach (var member in team.Members) member.TeamId = null;
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 5. POST: Salir (Igual que antes)
        [HttpPost("leave")]
        [Authorize(Roles = "Jugador")]
        public async Task<IActionResult> LeaveTeam()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user.TeamId == null) return BadRequest(new { message = "No tienes equipo." });

            var team = await _context.Teams.FindAsync(user.TeamId);
            if (team != null && team.CaptainId == userId) return BadRequest(new { message = "El capitán no puede abandonar." });

            user.TeamId = null;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Saliste del equipo." });
        }
    }
}