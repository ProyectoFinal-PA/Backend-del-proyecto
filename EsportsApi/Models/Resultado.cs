// En Models/Resultado.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace EsportsApi.Models
{
    public class Resultado
    {
        public int Id { get; set; }

        // Relación con la Partida (el resultado es de UNA partida)
        public int PartidaId { get; set; }
        [ForeignKey("PartidaId")]
        public virtual Partida Partida { get; set; }

        public int ScoreTeamA { get; set; }
        public int ScoreTeamB { get; set; }
        
        public int? WinnerTeamId { get; set; } // El Id del equipo ganador
    }
}