namespace WebApi.Security;

/// <summary>
/// Nombres de los roles de la aplicacion. Se usan tanto en el seeder como en
/// los atributos [Authorize(Roles = ...)].
///
/// Las propiedades compuestas (AdminOrDocente, Todos) se definen como
/// concatenacion de constantes porque los atributos de C# requieren
/// expresiones constantes en tiempo de compilacion.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Docente = "Docente";
    public const string Estudiante = "Estudiante";

    public const string AdminOrDocente = Admin + "," + Docente;
    public const string AdminOrDocenteOrEstudiante = Admin + "," + Docente + "," + Estudiante;

    public static readonly string[] All = { Admin, Docente, Estudiante };
}
