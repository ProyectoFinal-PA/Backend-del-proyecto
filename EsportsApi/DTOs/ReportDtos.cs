using System.ComponentModel.DataAnnotations;

namespace EsportsApi.DTOs
{
    public record CreateReportDto(
        [Required] int TournamentId,
        [Required] string Reason,
        [Required] string Description
    );
}