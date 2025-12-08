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
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mi Perfil
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.Team)
                .ThenInclude(t => t.Tournament)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            int matchesCount = 0;
            if (user.TeamId != null)
            {
                matchesCount = await _context.Partidas
                    .CountAsync(p => (p.TeamA_Id == user.TeamId || p.TeamB_Id == user.TeamId) 
                                     && p.Status == "Jugada");
            }

            // Enviamos el AvatarId también
            var profile = new UserProfileDto(
                user.Nickname,
                user.Email,
                user.Role,
                user.Team?.Name ?? "Sin Equipo", 
                user.Team?.Tournament?.Name ?? "Ninguno",
                matchesCount,
                user.AvatarId // <--- NUEVO
            );

            return Ok(profile);
        }

        // PUT: Cambiar Avatar (¡NUEVO!)
        [HttpPut("avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] UpdateAvatarDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            user.AvatarId = dto.AvatarId; // Actualizamos
            await _context.SaveChangesAsync();

            return Ok(new { message = "Avatar actualizado" });
        }
    }

    // DTOs
    public record UserProfileDto(
        string Nickname, 
        string Email, 
        string Role, 
        string? TeamName, 
        string? TournamentName, 
        int MatchesPlayed,
        string AvatarId // <--- Agregado al DTO
    );

    public record UpdateAvatarDto(string AvatarId);
}