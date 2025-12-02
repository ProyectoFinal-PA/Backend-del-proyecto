using System.ComponentModel.DataAnnotations;

namespace EsportsApi.DTOs
{
    
    public class TournamentCreateDto
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Game { get; set; }
        
        public string? GameImageUrl { get; set; } 

        [Required]
        public DateTime StartDate { get; set; }
        
        public string? KickChannel { get; set; } 
        public string? Prize { get; set; }
        public string? Rules { get; set; }
    }

   
    public class TournamentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Game { get; set; }
        public string? GameImageUrl { get; set; }
        public DateTime StartDate { get; set; }
        public string? KickChannel { get; set; }
        public string? Prize { get; set; }
        public string? Rules { get; set; }
        public string OrganizerNickname { get; set; }
    }
}