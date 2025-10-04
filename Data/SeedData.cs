using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public static class SeedData
    {
        private const string RolCoordinador = "Coordinador";
        private const string EmailCoordinador = "coordinador@demo.com";
        private const string PassCoordinador  = "Passw0rd!";

        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Migraciones (por si acaso)
            await context.Database.MigrateAsync();

            // Rol Coordinador
            if (!await roleManager.RoleExistsAsync(RolCoordinador))
                await roleManager.CreateAsync(new IdentityRole(RolCoordinador));

            // Usuario Coordinador
            var user = await userManager.FindByEmailAsync(EmailCoordinador);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = EmailCoordinador,
                    Email = EmailCoordinador,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, PassCoordinador);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RolCoordinador);
                }
                else
                {
                    throw new Exception("No se pudo crear el usuario Coordinador: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Asegurar rol
                if (!await userManager.IsInRoleAsync(user, RolCoordinador))
                    await userManager.AddToRoleAsync(user, RolCoordinador);
            }

            // Semillas de Cursos (si no existen)
            if (!await context.Cursos.AnyAsync())
            {
                var cursos = new List<Curso>
                {
                    new Curso {
                        Codigo = "CS101",
                        Nombre = "Algoritmos y Programación",
                        Creditos = 3,
                        CupoMaximo = 30,
                        HorarioInicio = new TimeSpan(8, 0, 0),
                        HorarioFin = new TimeSpan(10, 0, 0),
                        Activo = true
                    },
                    new Curso {
                        Codigo = "DB201",
                        Nombre = "Base de Datos",
                        Creditos = 4,
                        CupoMaximo = 25,
                        HorarioInicio = new TimeSpan(10, 0, 0),
                        HorarioFin = new TimeSpan(12, 0, 0),
                        Activo = true
                    },
                    new Curso {
                        Codigo = "UX110",
                        Nombre = "Diseño de Experiencia de Usuario",
                        Creditos = 2,
                        CupoMaximo = 20,
                        HorarioInicio = new TimeSpan(14, 0, 0),
                        HorarioFin = new TimeSpan(16, 0, 0),
                        Activo = true
                    }
                };

                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();
            }
        }
    }
}
