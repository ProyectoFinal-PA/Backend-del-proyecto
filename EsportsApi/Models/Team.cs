// En Models/Team.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Relación con el Torneo al que pertenece
        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }

        // --- Relaciones de Usuario (CORREGIDAS) ---

        // 1. Relación con el Capitán (un User)
        public int CaptainId { get; set; }
        [ForeignKey("CaptainId")]
        [InverseProperty("TeamsCaptained")] // <-- Cable que conecta a la lista de "TeamsCaptained"
        public virtual User Captain { get; set; }

        // 2. Relación con los Miembros (muchos Users)
        [InverseProperty("Team")] // <-- Cable que conecta a la propiedad "Team" del miembro
        public virtual ICollection<User> Members { get; set; } = new List<User>();
    }
}