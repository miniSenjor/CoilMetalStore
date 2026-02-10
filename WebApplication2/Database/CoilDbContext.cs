using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Database
{
    public class CoilDbContext : DbContext
    {
        public CoilDbContext(DbContextOptions<CoilDbContext> options) : base(options)
        { }
        public DbSet<Coil> Coils { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
