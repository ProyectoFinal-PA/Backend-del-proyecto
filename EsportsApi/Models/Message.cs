using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Quién lo envió
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User Sender { get; set; }

        // En qué torneo
        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }
    }
}