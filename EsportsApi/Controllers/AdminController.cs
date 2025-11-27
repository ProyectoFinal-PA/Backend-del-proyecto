using EsportsApi.Data;
using EsportsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EsportsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. OBTENER ESTADÍSTICAS (MEJORADO) ---
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // Total de Usuarios registrados
            var totalUsers = await _context.Users.CountAsync();
            
            // Total de Torneos creados
            var totalTournaments = await _context.Tournaments.CountAsync();
            
            // --- CAMBIO AQUÍ: CONTAMOS ORGANIZADORES ---
            var totalOrganizers = await _context.Users.CountAsync(u => u.Role == "Organizador");
            // -------------------------------------------

            return Ok(new DashboardStatsDto(totalUsers, totalTournaments, totalOrganizers));
        }

        // --- 2. Ver todos los usuarios ---
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Nickname, u.Email, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        // --- 3. Promover un Jugador a Organizador ---
        [HttpPost("promote/{userId}")]
        public async Task<IActionResult> PromoteToOrganizador(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Usuario no encontrado.");

            if (user.Role == "Jugador")
            {
                user.Role = "Organizador";
                await _context.SaveChangesAsync();
                return Ok(new { message = $"El usuario {user.Nickname} ahora es Organizador." });
            }
            return BadRequest($"El usuario ya es {user.Role}.");
        }
        
        // --- 4. Degradar un Organizador a Jugador ---
        [HttpPost("demote/{userId}")]
        public async Task<IActionResult> DemoteToJugador(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Usuario no encontrado.");

            if (user.Role == "Organizador")
            {
                user.Role = "Jugador";
                await _context.SaveChangesAsync();
                return Ok(new { message = $"El usuario {user.Nickname} ahora es Jugador." });
            }
            return BadRequest($"El usuario no es un Organizador.");
        }
    }

    // DTO Actualizado: Cambiamos MatchesPlayed por TotalOrganizers
    public record DashboardStatsDto(int TotalUsers, int TotalTournaments, int TotalOrganizers);
}