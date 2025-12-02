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

        // 1. GET (Leer) - Con Estado, Premios, Reglas e IMAGEN
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTournaments()
        {
            var tournaments = await _context.Tournaments
                .Include(t => t.Partidas)
                .Select(t => new 
                {
                    t.Id,
                    t.Name,
                    t.Game,
                    t.GameImageUrl, // <--- Importante para mostrar la imagen en el front
                    t.StartDate,
                    t.KickChannel,
                    t.Prize, 
                    t.Rules, 
                    OrganizadorNickname = t.Organizador.Nickname,

                    // Cálculo del Estado
                    Status = t.Partidas.Count == 0 
                        ? "Inscripciones" 
                        : (t.Partidas.Any(p => p.NextMatchId == null && p.Status == "Jugada") 
                            ? "Finalizado" 
                            : "En Juego")
                })
                .ToListAsync();
            
            return Ok(tournaments);
        }

        // 2. POST (Crear)
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
                GameImageUrl = dto.GameImageUrl, // <--- Guardamos la imagen
                StartDate = dto.StartDate,
                KickChannel = dto.KickChannel,
                Prize = dto.Prize,
                Rules = dto.Rules,
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
            tournament.GameImageUrl = dto.GameImageUrl; // <--- Actualizamos la imagen
            tournament.StartDate = dto.StartDate;
            tournament.KickChannel = dto.KickChannel;
            tournament.Prize = dto.Prize;
            tournament.Rules = dto.Rules;
            
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

        // 5. GET: SALÓN DE LA FAMA (Campeones)
        [HttpGet("hall-of-fame")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHallOfFame()
        {
            var finals = await _context.Partidas
                .Include(p => p.Tournament)
                .Include(p => p.Resultado)
                .Include(p => p.TeamA)
                .Include(p => p.TeamB)
                .Where(p => p.Label.Contains("FINAL") && p.Status == "Jugada")
                .ToListAsync();

            var hallOfFame = new List<object>();

            foreach (var partida in finals)
            {
                if (partida.Resultado != null && partida.Resultado.WinnerTeamId != null)
                {
                    string winnerName = "Desconocido";
                    if (partida.Resultado.WinnerTeamId == partida.TeamA_Id)
                        winnerName = partida.TeamA.Name;
                    else if (partida.Resultado.WinnerTeamId == partida.TeamB_Id)
                        winnerName = partida.TeamB.Name;

                    hallOfFame.Add(new
                    {
                        TournamentName = partida.Tournament.Name,
                        Game = partida.Tournament.Game,
                        WinnerTeam = winnerName,
                        Date = partida.ScheduledTime
                    });
                }
            }

            return Ok(hallOfFame);
        }
    }

    // DTO con GameImageUrl incluido
    public record TournamentCreateDto(
        string Name, 
        string Game, 
        string? GameImageUrl, 
        DateTime StartDate, 
        string? KickChannel, 
        string? Prize, 
        string? Rules 
    );
}