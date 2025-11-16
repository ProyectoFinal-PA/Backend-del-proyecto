// En Controllers/TournamentsController.cs
using EsportsApi.Data;
using EsportsApi.Models; // ¡Importante!
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // ¡¡MUY IMPORTANTE para leer el token!!

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

        // --- 1. GET (Leer) ---
        // Cualquiera puede ver los torneos
        [HttpGet]
        [AllowAnonymous] // Opuesto a [Authorize], permite a todos
        public async Task<IActionResult> GetTournaments()
        {
            // Borramos el código de prueba que crea un torneo
            var tournaments = await _context.Tournaments
                .Select(t => new // Seleccionamos solo los datos que queremos
                {
                    t.Id,
                    t.Name,
                    t.Game,
                    t.StartDate,
                    OrganizadorNickname = t.Organizador.Nickname // Mostramos el apodo del dueño
                })
                .ToListAsync();
            
            return Ok(tournaments);
        }

        // --- 2. POST (Crear) ---
        // Solo los "Organizadores" pueden crear torneos
        [HttpPost]
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> CreateTournament([FromBody] TournamentCreateDto dto)
        {
            var organizadorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(organizadorIdStr))
            {
                return Unauthorized(); 
            }
            
            var organizadorId = int.Parse(organizadorIdStr);

            var newTournament = new Tournament
            {
                Name = dto.Name,
                Game = dto.Game,
                StartDate = dto.StartDate,
                OrganizadorId = organizadorId // Asignamos el torneo al usuario logueado
            };

            _context.Tournaments.Add(newTournament);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTournaments), new { id = newTournament.Id }, newTournament);
        }

        // --- 3. PUT (Actualizar) ---
        // Solo "Admins" o el "Organizador dueño" pueden actualizar
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Organizador")] // Dejamos entrar a ambos roles
        public async Task<IActionResult> UpdateTournament(int id, [FromBody] TournamentCreateDto dto)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(userIdStr);

            if (tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid(); // Error 403 (Prohibido)
            }
            
            tournament.Name = dto.Name;
            tournament.Game = dto.Game;
            tournament.StartDate = dto.StartDate;
            await _context.SaveChangesAsync();

            return Ok(tournament);
        }

        // --- 4. DELETE (Borrar) ---
        // Solo "Admins" o el "Organizador dueño" pueden borrar
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Organizador")]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(userIdStr);

            if (tournament.OrganizadorId != userId && userRole != "Admin")
            {
                return Forbid(); // Error 403 (Prohibido)
            }

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 (Éxito sin contenido)
        }
    }

    // --- DTO (Data Transfer Object) ---
    public record TournamentCreateDto(string Name, string Game, DateTime StartDate);
}