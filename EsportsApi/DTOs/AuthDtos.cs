// En DTOs/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace EsportsApi.DTOs
{
    //                                                                AÑADIMOS ESTO vvvvvvvvvv
    public record RegisterDto([Required] string Email, [Required] string Password, [Required] string Nickname, string? Role);
    public record LoginDto([Required] string Email, [Required] string Password);
    public record LoginResponseDto(string Token);
}