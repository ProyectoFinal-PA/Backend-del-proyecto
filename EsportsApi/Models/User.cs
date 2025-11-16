// En Models/User.cs
using System.Collections.Generic; // <-- ESTA LÍNEA ES IMPORTANTE

namespace EsportsApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }

        // --- LÍNEA MODIFICADA ---
        public virtual ICollection<Tournament> TorneosOrganizados { get; set; } = new List<Tournament>();
    }
}