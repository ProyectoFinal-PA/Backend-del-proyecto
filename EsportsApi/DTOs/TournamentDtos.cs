using System.ComponentModel.DataAnnotations;

namespace EsportsApi.DTOs
{
    // DTO para CREAR un torneo (lo que envía el frontend)
    public class TournamentCreateDto
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Game { get; set; }
        
        public string? GameImageUrl { get; set; } // <-- EL CAMPO NUEVO

        [Required]
        public DateTime StartDate { get; set; }
        
        public string? KickChannel { get; set; } // Ojo: En el modelo se llama KickChannel
        public string? Prize { get; set; }
        public string? Rules { get; set; }
    }

    // DTO para LEER un torneo (lo que devuelve el backend)
    // (Opcional, por si queremos separar la lectura de la escritura en el futuro)
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