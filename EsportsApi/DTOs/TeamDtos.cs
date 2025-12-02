using System.Collections.Generic;

namespace EsportsApi.DTOs
{
    public record CreateTeamDto(string Name, int TournamentId, string? LogoUrl);

    public record TeamResponseDto(
        int Id,
        string Name,
        string? LogoUrl, // <--- The missing piece
        int TournamentId,
        string TournamentName,
        int CaptainId,
        string CaptainNickname,
        List<string> MemberNicknames
    );
}