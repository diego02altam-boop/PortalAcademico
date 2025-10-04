using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Codigo { get; set; } = null!; // Único

        [Required, StringLength(120)]
        public string Nombre { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Créditos debe ser > 0")]
        public int Creditos { get; set; }

        [Range(1, 1000, ErrorMessage = "Cupo máximo debe ser > 0")]
        public int CupoMaximo { get; set; }

        // Usamos TimeSpan para horario (solo hora/minutos)
        [Required]
        public TimeSpan HorarioInicio { get; set; }

        [Required]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;

        // Navegación opcional
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}
