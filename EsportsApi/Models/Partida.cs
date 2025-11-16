// En Models/Partida.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Partida
    {
        public int Id { get; set; }

        // Relación con el Torneo
        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }

        // Relación con Equipo A
        public int? TeamA_Id { get; set; } // Puede ser nulo al inicio
        [ForeignKey("TeamA_Id")]
        public virtual Team TeamA { get; set; }

        // Relación con Equipo B
        public int? TeamB_Id { get; set; } // Puede ser nulo al inicio
        [ForeignKey("TeamB_Id")]
        public virtual Team TeamB { get; set; }
        
        public DateTime ScheduledTime { get; set; } // Fecha y hora
        public string Status { get; set; } // Ej: "Pendiente", "Jugada"

        // Relación con el Resultado (una partida tiene un resultado)
        public virtual Resultado Resultado { get; set; }
    }
}