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
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. POST: Crear una denuncia (Solo Jugadores)
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var report = new Report
            {
                ReporterId = userId,
                TournamentId = dto.TournamentId,
                Reason = dto.Reason,
                Description = dto.Description,
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Denuncia enviada correctamente." });
        }

        // 2. GET: Ver todas las denuncias (Solo Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)    // Datos del que denuncia
                .Include(r => r.Tournament)  // Datos del torneo denunciado
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    ReporterName = r.Reporter.Nickname,
                    TournamentName = r.Tournament.Name,
                    r.Reason,
                    r.Description,
                    Date = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }
    }
}