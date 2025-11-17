// En DTOs/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace EsportsApi.DTOs
{
    public record RegisterDto([Required] string Email, [Required] string Password, [Required] string Nickname);
    public record LoginDto([Required] string Email, [Required] string Password);
    
    // --- ¡¡CAMBIO AQUÍ!! ---
    // Ahora devolvemos el Token Y el Rol
    public record LoginResponseDto(string Token, string Role);
}