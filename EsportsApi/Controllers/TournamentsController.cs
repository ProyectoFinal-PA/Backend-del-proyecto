// En Controllers/TournamentsController.cs
using EsportsApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/Tournaments
        [HttpGet]
        [Authorize(Roles = "Jugador")] // ¡PROTEGIDO! Solo roles "Jugador" pueden entrar
        public async Task<IActionResult> GetTournaments()
        {
            // (Para probar, creamos un torneo si no hay ninguno)
            if (!await _context.Tournaments.AnyAsync())
            {
                _context.Tournaments.Add(new Models.Tournament
                {
                    Name = "Torneo de Prueba",
                    Game = "League of Legends",
                    StartDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            
            var tournaments = await _context.Tournaments.ToListAsync();
            return Ok(tournaments);
        }
    }
}