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
    }
}