using PortalAcademico.Models;

namespace PortalAcademico.Services
{
    public interface ICursoCacheService
    {
        Task<List<Curso>> GetActivosCachedAsync();
        Task InvalidateAsync(); // lo llamaremos en P5 (crear/editar curso)
    }
}
