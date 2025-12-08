using System.ComponentModel.DataAnnotations;

namespace EsportsApi.DTOs
{
    public record SendMessageDto([Required] string Text, [Required] int TournamentId);
    
    public record MessageDto(
        int Id, 
        string Text, 
        string SenderName, 
        string Role, // Para pintar de color si es Admin u Organizador
        DateTime SentAt
    );
}