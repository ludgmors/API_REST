namespace WebApi.Models;

/// <summary>
/// Entidad intermedia (entidad "Asignacion" del diagrama).
/// Relaciona un Estudiante, un Curso y un Docente mediante tres claves foraneas.
/// </summary>
public class Asignacion
{
    public int AsignacionId { get; set; }

    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = null!;

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    public int DocenteId { get; set; }
    public Docente Docente { get; set; } = null!;
}
