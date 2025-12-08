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
        public string? TwitchChannelName { get; set; }
        
        public virtual Resultado Resultado { get; set; }

        // --- CAMPOS NUEVOS PARA EL BRACKET ---
        public int Round { get; set; } // 1=Octavos, 2=Cuartos, etc.
        public string Label { get; set; } // "Final", "Semifinal A", etc.
        public int? NextMatchId { get; set; } // ID de la partida donde irá el ganador
        // -------------------------------------
    }
}