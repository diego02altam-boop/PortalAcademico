using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    [Authorize] // requisito: debe estar autenticado
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext ctx, UserManager<IdentityUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        // POST: /Matriculas/Inscribirse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            // 1) Usuario
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Debes iniciar sesión para inscribirte.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // 2) Curso
            var curso = await _ctx.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
            if (curso == null)
            {
                TempData["Error"] = "El curso no existe o está inactivo.";
                return RedirectToAction("Index", "Cursos");
            }

            // 3) Ya matriculado al mismo curso (Pendiente o Confirmada)
            var yaMatriculado = await _ctx.Matriculas
                .AnyAsync(m => m.CursoId == cursoId
                            && m.UsuarioId == user.Id
                            && (m.Estado == EstadoMatricula.Pendiente || m.Estado == EstadoMatricula.Confirmada));

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado (o en proceso) en este curso.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // 4) Cupo (cuenta matrículas Pendiente o Confirmada)
            var ocupados = await _ctx.Matriculas
                .CountAsync(m => m.CursoId == cursoId
                              && (m.Estado == EstadoMatricula.Pendiente || m.Estado == EstadoMatricula.Confirmada));

            if (ocupados >= curso.CupoMaximo)
            {
                TempData["Error"] = "No hay cupos disponibles para este curso.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // 5) Solape horario con otras matrículas del usuario (Pendiente/Confirmada)
            var cursosUsuario = await _ctx.Matriculas
                .Where(m => m.UsuarioId == user.Id
                         && (m.Estado == EstadoMatricula.Pendiente || m.Estado == EstadoMatricula.Confirmada))
                .Select(m => m.CursoId)
                .ToListAsync();

            if (cursosUsuario.Count > 0)
            {
                var otros = await _ctx.Cursos
                    .Where(c => cursosUsuario.Contains(c.Id))
                    .Select(c => new { c.Nombre, c.HorarioInicio, c.HorarioFin })
                    .ToListAsync();

                bool solapa = otros.Any(o =>
                    // intervalos [inicio, fin) solapan si:
                    curso.HorarioInicio < o.HorarioFin && o.HorarioInicio < curso.HorarioFin);

                if (solapa)
                {
                    TempData["Error"] = "Este curso se solapa en horario con otra matrícula vigente.";
                    return RedirectToAction("Details", "Cursos", new { id = cursoId });
                }
            }

            // 6) Crear la matrícula (Pendiente)
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = user.Id,
                FechaRegistro = DateTime.UtcNow,
                Estado = EstadoMatricula.Pendiente
            };

            try
            {
                _ctx.Matriculas.Add(matricula);
                await _ctx.SaveChangesAsync();

                TempData["Ok"] = $"Te inscribiste a \"{curso.Nombre}\". Tu estado es PENDIENTE.";
            }
            catch (DbUpdateException)
            {
                // por si el índice único (CursoId, UsuarioId) dispara
                TempData["Error"] = "No se pudo completar la inscripción. Inténtalo nuevamente.";
            }

            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }
    }
}
