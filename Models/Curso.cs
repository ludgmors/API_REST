namespace WebApi.Models;

/// <summary>
/// Representa un curso ofrecido (entidad "Curso" del diagrama).
/// Relacion 1..* con Asignacion.
/// </summary>
public class Curso
{
    public int CursoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}
