using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using PortalAcademico.Services;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    [Route("Coordinador/Cursos/[action]")]
    public class AdminCursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICursoCacheService _catalogCache;

        public AdminCursosController(ApplicationDbContext context, ICursoCacheService catalogCache)
        {
            _context = context;
            _catalogCache = catalogCache;
        }

        // GET /Coordinador/Cursos
        [HttpGet, Route("/Coordinador/Cursos")]
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos
                .AsNoTracking()
                .OrderBy(c => c.Codigo)
                .ToListAsync();
            return View(cursos);
        }

        // GET /Coordinador/Cursos/Details/5
        [HttpGet("{id:int?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var curso = await _context.Cursos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        // GET /Coordinador/Cursos/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST /Coordinador/Cursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            if (curso.HorarioFin <= curso.HorarioInicio)
                ModelState.AddModelError(nameof(Curso.HorarioFin), "La hora de fin debe ser mayor a la de inicio.");

            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                await _catalogCache.InvalidateAsync();
                TempData["Ok"] = "Curso creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET /Coordinador/Cursos/Edit/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        // POST /Coordinador/Cursos/Edit/5
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (curso.HorarioFin <= curso.HorarioInicio)
                ModelState.AddModelError(nameof(Curso.HorarioFin), "La hora de fin debe ser mayor a la de inicio.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                    await _catalogCache.InvalidateAsync();
                    TempData["Ok"] = "Curso actualizado.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Cursos.AnyAsync(e => e.Id == curso.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET /Coordinador/Cursos/Delete/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var curso = await _context.Cursos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        // POST /Coordinador/Cursos/Delete/5
        [HttpPost("{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
                await _catalogCache.InvalidateAsync();
                TempData["Ok"] = "Curso eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------- Matrículas ----------

        // GET /Coordinador/Cursos/Matriculas?cursoId=5
        [HttpGet]
        public async Task<IActionResult> Matriculas(int cursoId)
        {
            var curso = await _context.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursoId);
            if (curso == null) return NotFound();

            var mats = await (from m in _context.Matriculas
                              join u in _context.Users on m.UsuarioId equals u.Id
                              where m.CursoId == cursoId
                              orderby m.FechaRegistro descending
                              select new MatriculaVM
                              {
                                  Id = m.Id,
                                  Usuario = u.Email!,
                                  Estado = m.Estado,
                                  FechaRegistro = m.FechaRegistro
                              }).ToListAsync();

            ViewBag.Curso = curso;
            return View(mats);
        }

        // POST /Coordinador/Cursos/Confirmar   (id en el body)
        [HttpPost("Confirmar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int id)
        {
            var m = await _context.Matriculas.FindAsync(id);
            if (m == null) return NotFound();

            m.Estado = EstadoMatricula.Confirmada;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Matrícula confirmada.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = m.CursoId });
        }

        // POST /Coordinador/Cursos/Cancelar    (id en el body)
        [HttpPost("Cancelar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var m = await _context.Matriculas.FindAsync(id);
            if (m == null) return NotFound();

            m.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Matrícula cancelada.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = m.CursoId });
        }

        // VM para la vista Matriculas
        public class MatriculaVM
        {
            public int Id { get; set; }
            public string Usuario { get; set; } = "";
            public EstadoMatricula Estado { get; set; }
            public DateTime FechaRegistro { get; set; }
        }
    }
}
