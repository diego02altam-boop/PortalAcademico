using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;                  // ApplicationDbContext (solo lo usamos en Details)
using PortalAcademico.Services;              // ICursoCacheService
using PortalAcademico.ViewModels.Cursos;     // CatalogoFiltroVM

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly ICursoCacheService _catalogCache;

        public CursosController(ApplicationDbContext ctx, ICursoCacheService catalogCache)
        {
            _ctx = ctx;
            _catalogCache = catalogCache;
        }

        // GET: /Cursos
        public async Task<IActionResult> Index([FromQuery] CatalogoFiltroVM filtros)
        {
            // 1) Traer base desde Redis (cursos activos)
            var lista = await _catalogCache.GetActivosCachedAsync();

            // 2) Validación del modelo de filtros (server-side)
            if (!ModelState.IsValid)
            {
                ViewBag.Filtros = filtros;
                return View(lista.OrderBy(c => c.Nombre).ToList());
            }

            // 3) Aplicar filtros EN MEMORIA
            if (!string.IsNullOrWhiteSpace(filtros.Nombre))
            {
                var needle = filtros.Nombre.Trim();
                lista = lista.Where(c =>
                        c.Nombre.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                        c.Codigo.Contains(needle, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (filtros.CreditosMin.HasValue)
                lista = lista.Where(c => c.Creditos >= filtros.CreditosMin.Value).ToList();

            if (filtros.CreditosMax.HasValue)
                lista = lista.Where(c => c.Creditos <= filtros.CreditosMax.Value).ToList();

            if (filtros.HoraInicio.HasValue)
                lista = lista.Where(c => c.HorarioInicio >= filtros.HoraInicio.Value).ToList();

            if (filtros.HoraFin.HasValue)
                lista = lista.Where(c => c.HorarioFin <= filtros.HoraFin.Value).ToList();

            // 4) Orden final (en memoria; evita ORDER BY TimeSpan en SQLite)
            lista = lista.OrderBy(c => c.HorarioInicio)
                         .ThenBy(c => c.Nombre)
                         .ToList();

            ViewBag.Filtros = filtros;
            return View(lista);
        }

        // GET: /Cursos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // En Details sí usamos el DbContext directamente
            var curso = await _ctx.Cursos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (curso == null) return NotFound();

            // Guardar en sesión el último curso visitado
            HttpContext.Session.SetInt32("UltimoCursoId", curso.Id);
            HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

            return View(curso);
        }
    }
}
