using EsportsApi.Data;
using EsportsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EsportsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TournamentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET (Leer) - Incluimos Premio y Reglas
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTournaments()
        {
            var tournaments = await _context.Tournaments
                .Select(t => new 
                {
                    t.Id,
                    t.Name,
                    t.Game,
                    t.StartDate,
                    t.KickChannel,
                    t.Prize, // <--- Nuevo
                    t.Rules, // <--- Nuevo
                    OrganizadorNickname = t.Organizador.Nickname
                })
                .ToListAsync();
            
            return Ok(tournaments);
        }

        // 2. POST (Crear) - Guardamos Premio y Reglas
        [HttpPost]
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> CreateTournament([FromBody] TournamentCreateDto dto)
        {
            var organizadorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(organizadorIdStr)) return Unauthorized();
            
            var organizadorId = int.Parse(organizadorIdStr);

            var newTournament = new Tournament
            {
                Name = dto.Name,
                Game = dto.Game,
                StartDate = dto.StartDate,
                KickChannel = dto.KickChannel,
                Prize = dto.Prize, // <--- Guardar
                Rules = dto.Rules, // <--- Guardar
                OrganizadorId = organizadorId 
            };

            _context.Tournaments.Add(newTournament);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTournaments), new { id = newTournament.Id }, newTournament);
        }

        // 3. PUT (Actualizar)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> UpdateTournament(int id, [FromBody] TournamentCreateDto dto)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid();
            }
            
            tournament.Name = dto.Name;
            tournament.Game = dto.Game;
            tournament.StartDate = dto.StartDate;
            tournament.KickChannel = dto.KickChannel;
            tournament.Prize = dto.Prize; // <--- Actualizar
            tournament.Rules = dto.Rules; // <--- Actualizar
            await _context.SaveChangesAsync();

            return Ok(tournament);
        }

        // 4. DELETE (Borrar)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid();
            }

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // --- DTO ACTUALIZADO ---
    public record TournamentCreateDto(
        string Name, 
        string Game, 
        DateTime StartDate, 
        string? KickChannel,
        string? Prize, // <--- Nuevo
        string? Rules  // <--- Nuevo
    );
}