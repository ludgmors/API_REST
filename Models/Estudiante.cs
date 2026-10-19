namespace WebApi.Models;

/// <summary>
/// Representa a un estudiante (entidad "Estudiante" del diagrama).
/// Relacion 1..* con Asignacion.
/// </summary>
public class Estudiante
{
    public int EstudianteId { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}
