using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Security;

namespace WebApi.Controllers;

/// <summary>
/// CRUD REST de docentes.
/// Lectura: cualquier usuario autenticado.
/// Escritura: solo rol Admin.
/// </summary>
[ApiController]
[Route("api/docentes")]
[Authorize]
public class DocentesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DocentesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todos los docentes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocenteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocenteDto>>> GetAll()
    {
        var docentes = await _context.Docentes
            .AsNoTracking()
            .Select(d => new DocenteDto(d.DocenteId, d.Nombres, d.Apellidos))
            .ToListAsync();

        return Ok(docentes);
    }

    /// <summary>Obtiene un docente por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DocenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocenteDto>> GetById(int id)
    {
        var docente = await _context.Docentes
            .AsNoTracking()
            .Where(d => d.DocenteId == id)
            .Select(d => new DocenteDto(d.DocenteId, d.Nombres, d.Apellidos))
            .FirstOrDefaultAsync();

        return docente is null ? NotFound() : Ok(docente);
    }

    /// <summary>Crea un docente. Requiere rol Admin.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(DocenteDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DocenteDto>> Create([FromBody] DocenteRequestDto dto)
    {
        var docente = new Docente { Nombres = dto.Nombres, Apellidos = dto.Apellidos };
        _context.Docentes.Add(docente);
        await _context.SaveChangesAsync();

        var result = new DocenteDto(docente.DocenteId, docente.Nombres, docente.Apellidos);
        return CreatedAtAction(nameof(GetById), new { id = docente.DocenteId }, result);
    }

    /// <summary>Actualiza un docente. Requiere rol Admin.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] DocenteRequestDto dto)
    {
        var docente = await _context.Docentes.FindAsync(id);
        if (docente is null)
        {
            return NotFound();
        }

        docente.Nombres = dto.Nombres;
        docente.Apellidos = dto.Apellidos;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Elimina un docente. Requiere rol Admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var docente = await _context.Docentes.FindAsync(id);
        if (docente is null)
        {
            return NotFound();
        }

        var tieneAsignaciones = await _context.Asignaciones.AnyAsync(a => a.DocenteId == id);
        if (tieneAsignaciones)
        {
            return Conflict(new { mensaje = "No se puede eliminar el docente porque tiene asignaciones asociadas." });
        }

        _context.Docentes.Remove(docente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
