using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.ViewModels.Cursos
{
    public class CatalogoFiltroVM : IValidatableObject
    {
        public string? Nombre { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Créditos mínimos no puede ser negativo")]
        public int? CreditosMin { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Créditos máximos no puede ser negativo")]
        public int? CreditosMax { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? HoraInicio { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? HoraFin { get; set; }

        // Validaciones server-side adicionales
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CreditosMin.HasValue && CreditosMax.HasValue && CreditosMin > CreditosMax)
                yield return new ValidationResult("Créditos mínimos no puede ser mayor que créditos máximos",
                    new[] { nameof(CreditosMin), nameof(CreditosMax) });

            if (HoraInicio.HasValue && HoraFin.HasValue && HoraFin <= HoraInicio)
                yield return new ValidationResult("La hora de fin debe ser mayor a la hora de inicio",
                    new[] { nameof(HoraInicio), nameof(HoraFin) });
        }
    }
}
