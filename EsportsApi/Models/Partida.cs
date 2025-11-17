// En Models/Partida.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Partida
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }

        public int? TeamA_Id { get; set; }
        [ForeignKey("TeamA_Id")]
        public virtual Team TeamA { get; set; }

        public int? TeamB_Id { get; set; }
        [ForeignKey("TeamB_Id")]
        public virtual Team TeamB { get; set; }
        
        public DateTime ScheduledTime { get; set; }
        public string Status { get; set; } 

        public virtual Resultado Resultado { get; set; }

        // --- ¡¡LÍNEA NUEVA AÑADIDA!! ---
        public string? TwitchChannelName { get; set; } // El nombre del canal, ej: "ibai"
        // -------------------------------
    }
}