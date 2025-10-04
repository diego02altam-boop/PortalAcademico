using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Services
{
    public class CursoCacheService : ICursoCacheService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "catalogo_activos_v1";

        public CursoCacheService(ApplicationDbContext ctx, IDistributedCache cache)
        {
            _ctx = ctx;
            _cache = cache;
        }

        public async Task<List<Curso>> GetActivosCachedAsync()
        {
            string? json = null;

            // 1) Intentar leer de la cache (si falla, ignoramos)
            try
            {
                json = await _cache.GetStringAsync(CacheKey);
                if (!string.IsNullOrEmpty(json))
                    return JsonSerializer.Deserialize<List<Curso>>(json)!;
            }
            catch
            {
                // No interrumpir el flujo si Redis no responde
            }

            // 2) Consultar BD
            var data = await _ctx.Cursos
                .AsNoTracking()
                .Where(c => c.Activo)
                .ToListAsync();

            // 3) Intentar guardar en cache (si falla, también ignoramos)
            try
            {
                var opts = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(data), opts);
            }
            catch
            {
                // Ignorar error de cache
            }

            return data;
        }

        public async Task InvalidateAsync()
        {
            try
            {
                await _cache.RemoveAsync(CacheKey);
            }
            catch
            {
                // Si Redis no está, simplemente no hacemos nada
            }
        }
    }
}
