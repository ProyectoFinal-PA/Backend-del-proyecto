
using System;

namespace EsportsApi.DTOs
{
    
    public record CreatePartidaDto(
        int TournamentId,
        int TeamA_Id,
        int TeamB_Id,
        DateTime ScheduledTime,
        string? TwitchChannelName 
    );

    
    public record RegisterResultadoDto(
        int ScoreTeamA,
        int ScoreTeamB,
        int WinnerTeamId
    );
}