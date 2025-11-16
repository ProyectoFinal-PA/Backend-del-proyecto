// En DTOs/PartidaDtos.cs
using System;

namespace EsportsApi.DTOs
{
    // DTO para crear una nueva partida
    public record CreatePartidaDto(
        int TournamentId,
        int TeamA_Id,
        int TeamB_Id,
        DateTime ScheduledTime
    );

    // DTO para registrar un resultado
    public record RegisterResultadoDto(
        int ScoreTeamA,
        int ScoreTeamB,
        int WinnerTeamId
    );
}