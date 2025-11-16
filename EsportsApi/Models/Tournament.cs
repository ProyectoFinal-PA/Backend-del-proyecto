// En Models/Tournament.cs
using System.ComponentModel.DataAnnotations.Schema; // (Añadimos esto)

namespace EsportsApi.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Game { get; set; }
        public DateTime StartDate { get; set; }

        // --- LÍNEAS NUEVAS AÑADIDAS ---
        // Esto le dice a la BD quién es el "dueño"
        public int OrganizadorId { get; set; } 
        
        [ForeignKey("OrganizadorId")]
        public virtual User Organizador { get; set; } 
        // -------------------------------
    }
}