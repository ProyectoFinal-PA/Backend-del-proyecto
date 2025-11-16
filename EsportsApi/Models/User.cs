// En Models/User.cs
namespace EsportsApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Admin", "Organizador", "Jugador"
    }
}