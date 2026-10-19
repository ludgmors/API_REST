using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Security;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Autenticacion y gestion de usuarios con roles (Identity + JWT).
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    /// <summary>Registra un nuevo usuario y le asigna el rol indicado.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (!Roles.All.Contains(dto.Role))
        {
            return BadRequest(new
            {
                mensaje = $"Rol invalido. Roles permitidos: {string.Join(", ", Roles.All)}"
            });
        }

        if (!await _roleManager.RoleExistsAsync(dto.Role))
        {
            return BadRequest(new { mensaje = $"El rol '{dto.Role}' no existe." });
        }

        var existente = await _userManager.FindByEmailAsync(dto.Email);
        if (existente is not null)
        {
            return Conflict(new { mensaje = "El correo ya esta registrado." });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No se pudo crear el usuario.",
                errores = result.Errors.Select(e => e.Description)
            });
        }

        await _userManager.AddToRoleAsync(user, dto.Role);

        return await BuildAuthResponseAsync(user);
    }

    /// <summary>Inicia sesion y devuelve un token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized(new { mensaje = "Credenciales invalidas." });
        }

        return await BuildAuthResponseAsync(user);
    }

    /// <summary>Devuelve la informacion del usuario autenticado y sus roles.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoDto>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserInfoDto(user.Id, user.Email ?? string.Empty, user.Nombres, user.Apellidos, roles.ToList());
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
    {
        var token = await _tokenService.CreateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto(token.Token, token.ExpiresAtUtc, user.Email ?? string.Empty, roles.ToList());
    }
}
