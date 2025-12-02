using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Report
    {
        public int Id { get; set; }

        // Quién denuncia (El Jugador)
        public int ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public virtual User Reporter { get; set; }

        // Qué torneo denuncian
        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }

        public string Reason { get; set; } // El motivo seleccionado (ej: "Fraude")
        public string Description { get; set; } // La explicación detallada
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}