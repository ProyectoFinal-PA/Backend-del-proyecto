namespace EsportsApi.DTOs
{
    public record UserProfileDto(
        string Nickname,
        string Email,
        string Role,
        string? TeamName,       // Equipo actual
        string? TournamentName, // Torneo actual
        int MatchesPlayed       // Estadística
    );
}