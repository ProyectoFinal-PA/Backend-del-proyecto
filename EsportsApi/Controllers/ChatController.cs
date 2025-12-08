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
    [Authorize] // Hay que estar logueado para ver o escribir
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Obtener mensajes de un torneo
        [HttpGet("{tournamentId}")]
        public async Task<IActionResult> GetMessages(int tournamentId)
        {
            var messages = await _context.Messages
                .Where(m => m.TournamentId == tournamentId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt) // Los más viejos arriba, los nuevos abajo
                .Select(m => new MessageDto(
                    m.Id,
                    m.Text,
                    m.Sender.Nickname,
                    m.Sender.Role,
                    m.SentAt
                ))
                .ToListAsync();

            return Ok(messages);
        }

        // 2. POST: Enviar mensaje
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // (Opcional) Validar que el torneo existe
            if (!await _context.Tournaments.AnyAsync(t => t.Id == dto.TournamentId))
                return NotFound("Torneo no encontrado");

            var msg = new Message
            {
                Text = dto.Text,
                TournamentId = dto.TournamentId,
                UserId = userId,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mensaje enviado" });
        }
    }
}