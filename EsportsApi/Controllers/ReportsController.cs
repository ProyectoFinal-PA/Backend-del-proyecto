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

        // 1. POST: Crear denuncia
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
                CreatedAt = DateTime.Now,
                Status = "Pendiente" // Nace pendiente
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Denuncia enviada." });
        }

        // 2. GET: Ver denuncias (Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)    
                .Include(r => r.Tournament)  
                .OrderByDescending(r => r.CreatedAt) // Las más nuevas primero
                .Select(r => new
                {
                    r.Id,
                    ReporterName = r.Reporter.Nickname,
                    TournamentName = r.Tournament.Name,
                    r.Reason,
                    r.Description,
                    r.Status, // <-- Enviamos el estado
                    Date = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        // 3. PUT: Resolver Denuncia (¡NUEVO!)
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Status = "Resuelto";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Denuncia marcada como resuelta." });
        }
    }
}