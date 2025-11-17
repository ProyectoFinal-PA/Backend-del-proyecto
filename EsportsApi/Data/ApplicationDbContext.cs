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
        

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
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
            
        }
    }
}