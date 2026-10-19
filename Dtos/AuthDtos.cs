using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos;

/// <summary>Datos para registrar un usuario. El rol debe ser Admin, Docente o Estudiante.</summary>
public record RegisterDto(
    [Required, EmailAddress]
    string Email,

    [Required, MinLength(6)]
    string Password,

    [MaxLength(100)]
    string? Nombres,

    [MaxLength(100)]
    string? Apellidos,

    [Required]
    string Role);

/// <summary>Credenciales para iniciar sesion.</summary>
public record LoginDto(
    [Required, EmailAddress]
    string Email,

    [Required]
    string Password);

/// <summary>Respuesta del login/registro: token JWT y roles del usuario.</summary>
public record AuthResponseDto(
    string Token,
    DateTime ExpiresAtUtc,
    string Email,
    IReadOnlyList<string> Roles);

/// <summary>Datos del usuario autenticado (endpoint /api/auth/me).</summary>
public record UserInfoDto(
    string Id,
    string Email,
    string? Nombres,
    string? Apellidos,
    IReadOnlyList<string> Roles);
