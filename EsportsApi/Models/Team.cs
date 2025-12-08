using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        // --- NUEVO CAMPO ---
        public string? LogoUrl { get; set; } 
        // -------------------

        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }

        public int CaptainId { get; set; }
        [ForeignKey("CaptainId")]
        public virtual User Captain { get; set; }

        public virtual ICollection<User> Members { get; set; } = new List<User>();
    }
}