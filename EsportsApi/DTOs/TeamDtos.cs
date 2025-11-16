// En DTOs/TeamDtos.cs
using System.Collections.Generic;

namespace EsportsApi.DTOs
{
    // DTO para crear un equipo nuevo
    public record CreateTeamDto(string Name, int TournamentId);

    // DTO para mostrar la información de un equipo
    public record TeamResponseDto(
        int Id,
        string Name,
        int TournamentId,
        string TournamentName,
        int CaptainId,
        string CaptainNickname,
        List<string> MemberNicknames
    );
}