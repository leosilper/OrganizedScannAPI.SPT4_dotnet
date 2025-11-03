// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Domain.Entities;

namespace OrganizedScannApi.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<Portal> Portals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== USERS =====
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("USERS");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.Email).HasColumnName("EMAIL");
                e.Property(x => x.Password).HasColumnName("PASSWORD");
                e.Property(x => x.Role).HasColumnName("ROLE");

                // opcional: índice/unique de Email
                e.HasIndex(x => x.Email).IsUnique();
            });

            // ===== PORTALS =====
            modelBuilder.Entity<Portal>(e =>
            {
                e.ToTable("PORTALS");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.Name).HasColumnName("NAME");
                e.Property(x => x.Type).HasColumnName("TYPE");
                // se houver CreatedAt no domínio, mapeie:
                // e.Property<DateTime>("CreatedAt").HasColumnName("CREATED_AT");
            });

            // ===== MOTORCYCLES =====
            modelBuilder.Entity<Motorcycle>(e =>
            {
                e.ToTable("MOTORCYCLES");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.LicensePlate).HasColumnName("LICENSEPLATE");
                e.Property(x => x.Rfid).HasColumnName("RFID");
                e.Property(x => x.ProblemDescription).HasColumnName("PROBLEMDESCRIPTION");
                e.Property(x => x.PortalId).HasColumnName("PORTALID");
                e.Property(x => x.EntryDate).HasColumnName("ENTRYDATE");
                e.Property(x => x.AvailabilityForecast).HasColumnName("AVAILABILITYFORECAST");
                e.Property(x => x.Brand).HasColumnName("BRAND");
                e.Property(x => x.Year).HasColumnName("YEAR");

                // FK para PORTALS (se existir navegação Portal em Motorcycle)
                // e.HasOne(x => x.Portal).WithMany().HasForeignKey(x => x.PortalId);

                // opcionais: índices/unique como no DDL
                e.HasIndex(x => x.LicensePlate).IsUnique();
                e.HasIndex(x => x.Rfid).IsUnique();
            });

            // Se suas tabelas estão em outro SCHEMA, use:
            // modelBuilder.HasDefaultSchema("SEU_SCHEMA");
            // ou ToTable("TABELA", "SEU_SCHEMA") em cada entidade.
        }
    }
}
