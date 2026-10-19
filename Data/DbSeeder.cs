using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Security;

namespace WebApi.Data;

/// <summary>
/// Siembra inicial de la base de datos:
///   1. Aplica las migraciones pendientes.
///   2. Crea los roles Admin, Docente y Estudiante.
///   3. Crea un usuario por cada rol (credenciales de DESARROLLO).
///   4. Inserta datos de ejemplo (cursos, docentes, estudiantes y asignaciones).
///
/// Se ejecuta automaticamente al iniciar la aplicacion (ver Program.cs).
/// </summary>
public static class DbSeeder
{
    // Credenciales de los usuarios semilla. SOLO para desarrollo.
    public const string AdminEmail = "admin@webapi.local";
    public const string AdminPassword = "Admin123*";

    public const string DocenteEmail = "docente@webapi.local";
    public const string DocentePassword = "Docente123*";

    public const string EstudianteEmail = "estudiante@webapi.local";
    public const string EstudiantePassword = "Estudiante123*";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // 1. Aplicar migraciones (crea la base de datos si no existe).
        await context.Database.MigrateAsync();

        // 2. Roles.
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Rol creado: {Role}", role);
            }
        }

        // 3. Usuarios, cada uno con un rol diferente.
        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, Roles.Admin, "Admin", "Sistema");
        await EnsureUserAsync(userManager, DocenteEmail, DocentePassword, Roles.Docente, "Juan", "Perez");
        await EnsureUserAsync(userManager, EstudianteEmail, EstudiantePassword, Roles.Estudiante, "Maria", "Lopez");

        // 4. Datos de ejemplo del dominio.
        await SeedDomainDataAsync(context);

        logger.LogInformation("Siembra de datos finalizada.");
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        string nombres,
        string apellidos)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nombres = nombres,
                Apellidos = apellidos
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el usuario semilla '{email}': " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task SeedDomainDataAsync(ApplicationDbContext context)
    {
        if (await context.Cursos.AnyAsync())
        {
            return;
        }

        var cursoProgramacion = new Curso { Nombre = "Programacion I" };
        var cursoBd = new Curso { Nombre = "Base de Datos" };
        var cursoApi = new Curso { Nombre = "Web API REST" };
        context.Cursos.AddRange(cursoProgramacion, cursoBd, cursoApi);

        var docentePerez = new Docente { Nombres = "Juan", Apellidos = "Perez" };
        var docenteGomez = new Docente { Nombres = "Ana", Apellidos = "Gomez" };
        context.Docentes.AddRange(docentePerez, docenteGomez);

        var estudianteLopez = new Estudiante { Nombres = "Maria", Apellidos = "Lopez", Direccion = "Av. Siempre Viva 123" };
        var estudianteRamirez = new Estudiante { Nombres = "Carlos", Apellidos = "Ramirez", Direccion = "Calle Falsa 456" };
        context.Estudiantes.AddRange(estudianteLopez, estudianteRamirez);

        await context.SaveChangesAsync();

        context.Asignaciones.AddRange(
            new Asignacion
            {
                EstudianteId = estudianteLopez.EstudianteId,
                CursoId = cursoProgramacion.CursoId,
                DocenteId = docentePerez.DocenteId
            },
            new Asignacion
            {
                EstudianteId = estudianteLopez.EstudianteId,
                CursoId = cursoBd.CursoId,
                DocenteId = docenteGomez.DocenteId
            },
            new Asignacion
            {
                EstudianteId = estudianteRamirez.EstudianteId,
                CursoId = cursoProgramacion.CursoId,
                DocenteId = docentePerez.DocenteId
            });

        await context.SaveChangesAsync();
    }
}
