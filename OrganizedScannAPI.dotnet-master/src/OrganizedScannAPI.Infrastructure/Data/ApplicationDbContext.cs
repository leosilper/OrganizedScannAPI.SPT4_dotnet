// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OrganizedScannApi.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<Portal> Portals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações específicas de entidades, se necessário
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Motorcycle>().ToTable("Motorcycles");
            modelBuilder.Entity<Portal>().ToTable("Portals");
        }
    }
}