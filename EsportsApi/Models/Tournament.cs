using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Game { get; set; }
        public DateTime StartDate { get; set; }
        public string? KickChannel { get; set; }
        
        // --- NUEVOS CAMPOS ---
        public string? Prize { get; set; } // Premio (Opcional)
        public string? Rules { get; set; } // Reglas (Opcional)
        // ---------------------

        public int OrganizadorId { get; set; } 
        [ForeignKey("OrganizadorId")]
        public virtual User Organizador { get; set; } 

        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}