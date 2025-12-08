using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Game { get; set; }

        // --- NUEVO CAMPO (Para la imagen de RAWG) ---
        public string? GameImageUrl { get; set; } 
        // --------------------------------------------

        public DateTime StartDate { get; set; }
        
        public string? KickChannel { get; set; }
        
        // --- Campos de Información ---
        public string? Prize { get; set; } 
        public string? Rules { get; set; } 

        // --- Relaciones ---
        public int OrganizadorId { get; set; } 
        [ForeignKey("OrganizadorId")]
        public virtual User Organizador { get; set; } 

        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

        // Relación con Partidas (necesaria para el estado)
        public virtual ICollection<Partida> Partidas { get; set; } = new List<Partida>();
    }
}