// En Models/User.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; 

namespace EsportsApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }

        // Relación con Torneos que organiza
        public virtual ICollection<Tournament> TorneosOrganizados { get; set; } = new List<Tournament>();

        // --- Relaciones de Equipo (CORREGIDAS) ---

        // 1. Relación de MEMBRESÍA (a qué equipo pertenezco)
        public int? TeamId { get; set; } // El ID del equipo al que pertenezco
        
        [ForeignKey("TeamId")]
        [InverseProperty("Members")] // <-- Cable que conecta a la lista de Miembros
        public virtual Team Team { get; set; }

        // 2. Relación de CAPITÁN (de qué equipos soy capitán)
        [InverseProperty("Captain")] // <-- Cable que conecta al Capitán
        public virtual ICollection<Team> TeamsCaptained { get; set; } = new List<Team>();
        // -------------------------------
    }
}