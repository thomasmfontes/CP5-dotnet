using CP5.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CP5.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Filme> Filmes => Set<Filme>();
        public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
        public DbSet<User> Users => Set<User>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
