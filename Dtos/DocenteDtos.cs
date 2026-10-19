using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos;

/// <summary>Datos que la API devuelve de un docente.</summary>
public record DocenteDto(int DocenteId, string Nombres, string Apellidos);

/// <summary>Datos que el cliente envia para crear o actualizar un docente.</summary>
public record DocenteRequestDto(
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [MaxLength(100)]
    string Nombres,

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [MaxLength(100)]
    string Apellidos);
