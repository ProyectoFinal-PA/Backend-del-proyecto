using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Resultado
    {
        public int Id { get; set; }
        
        public int PartidaId { get; set; }
        [ForeignKey("PartidaId")]
        public virtual Partida Partida { get; set; }

        public int ScoreTeamA { get; set; }
        public int ScoreTeamB { get; set; }
        
        public int? WinnerTeamId { get; set; }
    }
}