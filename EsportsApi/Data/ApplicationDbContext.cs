// En Data/ApplicationDbContext.cs
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
        
        // --- LÍNEAS NUEVAS AÑADIDAS ---
        public DbSet<Partida> Partidas { get; set; }
        public DbSet<Resultado> Resultados { get; set; }
        // --------------------------------

        // Le decimos a EF cómo manejar las relaciones complejas
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Reglas para Equipos ---
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Captain)
                .WithMany(u => u.TeamsCaptained) // El User puede capitanear muchos equipos
                .HasForeignKey(t => t.CaptainId)
                .OnDelete(DeleteBehavior.NoAction); // ¡¡NO BORRAR EN CASCADA!!

            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Members) // El Equipo tiene muchos Miembros (User)
                .HasForeignKey(u => u.TeamId)
                .OnDelete(DeleteBehavior.NoAction); // ¡¡NO BORRACASCADA!!
            
            // --- LÍNEAS NUEVAS PARA PARTIDAS ---
            modelBuilder.Entity<Partida>()
                .HasOne(p => p.TeamA)
                .WithMany() // No necesitamos una colección en Team
                .HasForeignKey(p => p.TeamA_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Partida>()
                .HasOne(p => p.TeamB)
                .WithMany() // No necesitamos una colección en Team
                .HasForeignKey(p => p.TeamB_Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Una Partida tiene UN Resultado
            modelBuilder.Entity<Partida>()
                .HasOne(p => p.Resultado)
                .WithOne(r => r.Partida) // Un Resultado tiene UNA Partida
                .HasForeignKey<Resultado>(r => r.PartidaId);
            // ------------------------------------
        }
    }
}