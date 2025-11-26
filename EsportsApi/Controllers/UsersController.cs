using EsportsApi.Data;
using EsportsApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EsportsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Solo usuarios logueados pueden ver su perfil
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // 1. Saber quién soy
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 2. Buscar mis datos + Equipo + Torneo
            var user = await _context.Users
                .Include(u => u.Team)
                .ThenInclude(t => t.Tournament)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            // 3. Calcular estadísticas (Partidas jugadas por mi equipo)
            int matchesCount = 0;
            if (user.TeamId != null)
            {
                matchesCount = await _context.Partidas
                    .CountAsync(p => (p.TeamA_Id == user.TeamId || p.TeamB_Id == user.TeamId) 
                                     && p.Status == "Jugada");
            }

            // 4. Armar el DTO
            var profile = new UserProfileDto(
                user.Nickname,
                user.Email,
                user.Role,
                user.Team?.Name ?? "Sin Equipo", // Si es null, pone "Sin Equipo"
                user.Team?.Tournament?.Name ?? "Ninguno",
                matchesCount
            );

            return Ok(profile);
        }
    }
}