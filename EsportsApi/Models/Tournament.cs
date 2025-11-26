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
        
        // --- CAMPOS DE INFORMACIÓN ---
        public string? Prize { get; set; } 
        public string? Rules { get; set; } 
        // -----------------------------

        public int OrganizadorId { get; set; } 
        [ForeignKey("OrganizadorId")]
        public virtual User Organizador { get; set; } 

        // Relación con Equipos
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

        // --- ¡ESTA ES LA LÍNEA QUE FALTABA PARA CORREGIR EL ERROR! ---
        // Relación con Partidas (necesaria para calcular el estado "En Juego/Finalizado")
        public virtual ICollection<Partida> Partidas { get; set; } = new List<Partida>();
        // -------------------------------------------------------------
    }
}