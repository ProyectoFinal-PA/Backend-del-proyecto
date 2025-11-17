using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public int TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }
        
        public int CaptainId { get; set; }
        [ForeignKey("CaptainId")]
        [InverseProperty("TeamsCaptained")] 
        public virtual User Captain { get; set; }
        
        [InverseProperty("Team")] 
        public virtual ICollection<User> Members { get; set; } = new List<User>();
    }
}