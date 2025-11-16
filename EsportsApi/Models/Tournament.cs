// En Models/Tournament.cs
using System.ComponentModel.DataAnnotations.Schema; // <-- ESTA LÍNEA ES IMPORTANTE

namespace EsportsApi.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Game { get; set; }
        public DateTime StartDate { get; set; }

        // --- LÍNEAS NUEVAS AÑADIDAS ---
        public int OrganizadorId { get; set; } 
        
        [ForeignKey("OrganizadorId")]
        public virtual User Organizador { get; set; } 
        // -------------------------------
        // --- LÍNEA NUEVA AÑADIDA ---
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        // -------------------------------
    }
}