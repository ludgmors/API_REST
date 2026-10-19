using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos;

/// <summary>Datos que la API devuelve de un estudiante.</summary>
public record EstudianteDto(int EstudianteId, string Nombres, string Apellidos, string? Direccion);

/// <summary>Datos que el cliente envia para crear o actualizar un estudiante.</summary>
public record EstudianteRequestDto(
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [MaxLength(100)]
    string Nombres,

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [MaxLength(100)]
    string Apellidos,

    [MaxLength(200)]
    string? Direccion);
