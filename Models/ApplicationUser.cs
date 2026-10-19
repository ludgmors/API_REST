using Microsoft.AspNetCore.Identity;

namespace WebApi.Models;

/// <summary>
/// Usuario de la aplicacion (extiende IdentityUser para agregar datos propios).
/// Los roles se administran mediante ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }
}
