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

        
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Nickname, u.Email, u.Role })
                .ToListAsync();
            return Ok(users);
        }

       
        [HttpPost("promote/{userId}")]
        public async Task<IActionResult> PromoteToOrganizador(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            if (user.Role == "Jugador")
            {
                user.Role = "Organizador";
                await _context.SaveChangesAsync();
                return Ok(new { message = $"El usuario {user.Nickname} ahora es Organizador." });
            }

            return BadRequest($"El usuario ya es {user.Role}.");
        }
        
       
        [HttpPost("demote/{userId}")]
        public async Task<IActionResult> DemoteToJugador(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            if (user.Role == "Organizador")
            {
                user.Role = "Jugador";
                await _context.SaveChangesAsync();
                return Ok(new { message = $"El usuario {user.Nickname} ahora es Jugador." });
            }

            return BadRequest($"El usuario no es un Organizador.");
        }
    }
}