using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<Matricula> Matriculas => Set<Matricula>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Curso: Código único
            builder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            // Curso: check constraints (EF Core lo traduce a SQL)
            builder.Entity<Curso>()
                .ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Curso_Creditos_Pos", "Creditos > 0");
                    tb.HasCheckConstraint("CK_Curso_Cupo_Pos", "CupoMaximo > 0");
                    // HorarioInicio < HorarioFin (se guardan como texto/num según provider)
                    tb.HasCheckConstraint("CK_Curso_Horario", "HorarioInicio < HorarioFin");
                });

            // Matricula: un usuario no puede matricularse dos veces en el mismo curso
            builder.Entity<Matricula>()
                .HasIndex(m => new { m.CursoId, m.UsuarioId })
                .IsUnique();

            // Relación
            builder.Entity<Matricula>()
                .HasOne(m => m.Curso)
                .WithMany(c => c.Matriculas)
                .HasForeignKey(m => m.CursoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
