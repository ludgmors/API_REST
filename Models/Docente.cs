namespace WebApi.Models;

/// <summary>
/// Representa a un docente (entidad "Docente" del diagrama).
/// Relacion 1..* con Asignacion.
/// </summary>
public class Docente
{
    public int DocenteId { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}
