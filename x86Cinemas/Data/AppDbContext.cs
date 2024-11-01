using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using x86Cinemas.Models;

namespace x86Cinemas.Data
{
    public class AppDbContext: DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Usuarios>Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuarios>(tb =>
            {
                tb.HasKey(col => col.ID);
                tb.Property(col => col.ID).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.NombreCompleto).HasMaxLength(100);
                tb.Property(col => col.Correo).HasMaxLength(100);
                tb.Property(col => col.Clave).HasMaxLength(255);
                tb.Property(col => col.Administrador).HasMaxLength(3);

            });

            modelBuilder.Entity<Usuarios>().ToTable("USUARIOS");

        }

    }
}
