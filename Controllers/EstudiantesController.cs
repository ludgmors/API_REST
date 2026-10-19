using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Security;

namespace WebApi.Controllers;

/// <summary>
/// CRUD REST de estudiantes.
/// Lectura: roles Admin y Docente.
/// Escritura: solo rol Admin.
/// </summary>
[ApiController]
[Route("api/estudiantes")]
[Authorize(Roles = Roles.AdminOrDocente)]
public class EstudiantesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EstudiantesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todos los estudiantes. Requiere rol Admin o Docente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EstudianteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EstudianteDto>>> GetAll()
    {
        var estudiantes = await _context.Estudiantes
            .AsNoTracking()
            .Select(e => new EstudianteDto(e.EstudianteId, e.Nombres, e.Apellidos, e.Direccion))
            .ToListAsync();

        return Ok(estudiantes);
    }

    /// <summary>Obtiene un estudiante por su id. Requiere rol Admin o Docente.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EstudianteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteDto>> GetById(int id)
    {
        var estudiante = await _context.Estudiantes
            .AsNoTracking()
            .Where(e => e.EstudianteId == id)
            .Select(e => new EstudianteDto(e.EstudianteId, e.Nombres, e.Apellidos, e.Direccion))
            .FirstOrDefaultAsync();

        return estudiante is null ? NotFound() : Ok(estudiante);
    }

    /// <summary>Crea un estudiante. Requiere rol Admin.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(EstudianteDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EstudianteDto>> Create([FromBody] EstudianteRequestDto dto)
    {
        var estudiante = new Estudiante
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Direccion = dto.Direccion
        };

        _context.Estudiantes.Add(estudiante);
        await _context.SaveChangesAsync();

        var result = new EstudianteDto(estudiante.EstudianteId, estudiante.Nombres, estudiante.Apellidos, estudiante.Direccion);
        return CreatedAtAction(nameof(GetById), new { id = estudiante.EstudianteId }, result);
    }

    /// <summary>Actualiza un estudiante. Requiere rol Admin.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EstudianteRequestDto dto)
    {
        var estudiante = await _context.Estudiantes.FindAsync(id);
        if (estudiante is null)
        {
            return NotFound();
        }

        estudiante.Nombres = dto.Nombres;
        estudiante.Apellidos = dto.Apellidos;
        estudiante.Direccion = dto.Direccion;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Elimina un estudiante. Requiere rol Admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var estudiante = await _context.Estudiantes.FindAsync(id);
        if (estudiante is null)
        {
            return NotFound();
        }

        var tieneAsignaciones = await _context.Asignaciones.AnyAsync(a => a.EstudianteId == id);
        if (tieneAsignaciones)
        {
            return Conflict(new { mensaje = "No se puede eliminar el estudiante porque tiene asignaciones asociadas." });
        }

        _context.Estudiantes.Remove(estudiante);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
