using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; 

namespace EsportsApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        
        public string AvatarId { get; set; } = "default";
        
        public virtual ICollection<Tournament> TorneosOrganizados { get; set; } = new List<Tournament>();
        
        public int? TeamId { get; set; } 
        
        [ForeignKey("TeamId")]
        [InverseProperty("Members")] 
        public virtual Team Team { get; set; }
        
        [InverseProperty("Captain")] 
        public virtual ICollection<Team> TeamsCaptained { get; set; } = new List<Team>();
        
    }
}