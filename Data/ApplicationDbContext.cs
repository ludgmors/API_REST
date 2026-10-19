using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data;

/// <summary>
/// Contexto de Entity Framework Core (Code First).
/// Hereda de IdentityDbContext para incluir las tablas de usuarios y roles.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuracion de las entidades del dominio.
        builder.Entity<Curso>(entity =>
        {
            entity.HasKey(c => c.CursoId);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(150);
        });

        builder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.EstudianteId);
            entity.Property(e => e.Nombres).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(200);
        });

        builder.Entity<Docente>(entity =>
        {
            entity.HasKey(d => d.DocenteId);
            entity.Property(d => d.Nombres).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Apellidos).IsRequired().HasMaxLength(100);
        });

        builder.Entity<Asignacion>(entity =>
        {
            entity.HasKey(a => a.AsignacionId);

            // Un estudiante no puede tener el mismo curso asignado dos veces.
            entity.HasIndex(a => new { a.EstudianteId, a.CursoId }).IsUnique();

            entity.HasOne(a => a.Estudiante)
                  .WithMany(e => e.Asignaciones)
                  .HasForeignKey(a => a.EstudianteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Curso)
                  .WithMany(c => c.Asignaciones)
                  .HasForeignKey(a => a.CursoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Docente)
                  .WithMany(d => d.Asignaciones)
                  .HasForeignKey(a => a.DocenteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
