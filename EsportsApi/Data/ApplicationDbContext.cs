using EsportsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EsportsApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Partida> Partidas { get; set; }
        public DbSet<Resultado> Resultados { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Message> Messages { get; set; } // <-- Nueva tabla

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // --- Configuración de Equipos ---
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Captain)
                .WithMany(u => u.TeamsCaptained)
                .HasForeignKey(t => t.CaptainId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Members) 
                .HasForeignKey(u => u.TeamId)
                .OnDelete(DeleteBehavior.NoAction); 
            
            // --- Configuración de Partidas ---
            modelBuilder.Entity<Partida>()
                .HasOne(p => p.TeamA)
                .WithMany() 
                .HasForeignKey(p => p.TeamA_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Partida>()
                .HasOne(p => p.TeamB)
                .WithMany() 
                .HasForeignKey(p => p.TeamB_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Partida>()
                .HasOne(p => p.Resultado)
                .WithOne(r => r.Partida) 
                .HasForeignKey<Resultado>(r => r.PartidaId);

            // --- Configuración de Reportes ---
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Tournament)
                .WithMany()
                .HasForeignKey(r => r.TournamentId)
                .OnDelete(DeleteBehavior.NoAction); 

            // --- ¡ESTA ES LA CORRECCIÓN PARA LOS MENSAJES! ---
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Evita ciclo al borrar usuario

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Tournament)
                .WithMany()
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.NoAction); // Evita ciclo al borrar torneo
            // -------------------------------------------------
        }
    }
}