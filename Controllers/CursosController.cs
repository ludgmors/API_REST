using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Security;

namespace WebApi.Controllers;

/// <summary>
/// CRUD REST de cursos.
/// Lectura: cualquier usuario autenticado.
/// Escritura: solo rol Admin.
/// </summary>
[ApiController]
[Route("api/cursos")]
[Authorize]
public class CursosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todos los cursos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CursoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CursoDto>>> GetAll()
    {
        var cursos = await _context.Cursos
            .AsNoTracking()
            .Select(c => new CursoDto(c.CursoId, c.Nombre))
            .ToListAsync();

        return Ok(cursos);
    }

    /// <summary>Obtiene un curso por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CursoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CursoDto>> GetById(int id)
    {
        var curso = await _context.Cursos
            .AsNoTracking()
            .Where(c => c.CursoId == id)
            .Select(c => new CursoDto(c.CursoId, c.Nombre))
            .FirstOrDefaultAsync();

        return curso is null ? NotFound() : Ok(curso);
    }

    /// <summary>Crea un curso. Requiere rol Admin.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CursoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CursoDto>> Create([FromBody] CursoRequestDto dto)
    {
        var curso = new Curso { Nombre = dto.Nombre };
        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        var result = new CursoDto(curso.CursoId, curso.Nombre);
        return CreatedAtAction(nameof(GetById), new { id = curso.CursoId }, result);
    }

    /// <summary>Actualiza un curso. Requiere rol Admin.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CursoRequestDto dto)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso is null)
        {
            return NotFound();
        }

        curso.Nombre = dto.Nombre;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Elimina un curso. Requiere rol Admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso is null)
        {
            return NotFound();
        }

        var tieneAsignaciones = await _context.Asignaciones.AnyAsync(a => a.CursoId == id);
        if (tieneAsignaciones)
        {
            return Conflict(new { mensaje = "No se puede eliminar el curso porque tiene asignaciones asociadas." });
        }

        _context.Cursos.Remove(curso);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
