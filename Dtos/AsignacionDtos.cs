using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos;

/// <summary>Datos que la API devuelve de una asignacion (incluye nombres legibles).</summary>
public record AsignacionDto(
    int AsignacionId,
    int EstudianteId,
    string Estudiante,
    int CursoId,
    string Curso,
    int DocenteId,
    string Docente);

/// <summary>Datos que el cliente envia para crear o actualizar una asignacion.</summary>
public record AsignacionRequestDto(
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un EstudianteId valido.")]
    int EstudianteId,

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un CursoId valido.")]
    int CursoId,

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un DocenteId valido.")]
    int DocenteId);
