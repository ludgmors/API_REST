using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos;

/// <summary>Datos que la API devuelve de un curso.</summary>
public record CursoDto(int CursoId, string Nombre);

/// <summary>Datos que el cliente envia para crear o actualizar un curso.</summary>
public record CursoRequestDto(
    [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
    [MaxLength(150)]
    string Nombre);
