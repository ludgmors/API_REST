using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Security;

namespace WebApi.Controllers;

/// <summary>
/// CRUD REST de asignaciones (relacion Estudiante - Curso - Docente).
/// Lectura: roles Admin y Docente.
/// Escritura: solo rol Admin.
/// </summary>
[ApiController]
[Route("api/asignaciones")]
[Authorize(Roles = Roles.AdminOrDocente)]
public class AsignacionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AsignacionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todas las asignaciones. Requiere rol Admin o Docente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AsignacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AsignacionDto>>> GetAll()
    {
        var asignaciones = await _context.Asignaciones
            .AsNoTracking()
            .Select(a => new AsignacionDto(
                a.AsignacionId,
                a.EstudianteId,
                a.Estudiante.Nombres + " " + a.Estudiante.Apellidos,
                a.CursoId,
                a.Curso.Nombre,
                a.DocenteId,
                a.Docente.Nombres + " " + a.Docente.Apellidos))
            .ToListAsync();

        return Ok(asignaciones);
    }

    /// <summary>Obtiene una asignacion por su id. Requiere rol Admin o Docente.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AsignacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AsignacionDto>> GetById(int id)
    {
        var asignacion = await BuildDtoAsync(id);
        return asignacion is null ? NotFound() : Ok(asignacion);
    }

    /// <summary>Crea una asignacion. Requiere rol Admin.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(AsignacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AsignacionDto>> Create([FromBody] AsignacionRequestDto dto)
    {
        var existeEstudiante = await _context.Estudiantes.AnyAsync(e => e.EstudianteId == dto.EstudianteId);
        var existeCurso = await _context.Cursos.AnyAsync(c => c.CursoId == dto.CursoId);
        var existeDocente = await _context.Docentes.AnyAsync(d => d.DocenteId == dto.DocenteId);

        if (!existeEstudiante || !existeCurso || !existeDocente)
        {
            return BadRequest(new { mensaje = "El EstudianteId, CursoId o DocenteId no existen." });
        }

        var yaExiste = await _context.Asignaciones.AnyAsync(a =>
            a.EstudianteId == dto.EstudianteId && a.CursoId == dto.CursoId);

        if (yaExiste)
        {
            return Conflict(new { mensaje = "El estudiante ya tiene asignado ese curso." });
        }

        var asignacion = new Asignacion
        {
            EstudianteId = dto.EstudianteId,
            CursoId = dto.CursoId,
            DocenteId = dto.DocenteId
        };

        _context.Asignaciones.Add(asignacion);
        await _context.SaveChangesAsync();

        var creada = await BuildDtoAsync(asignacion.AsignacionId);
        return CreatedAtAction(nameof(GetById), new { id = asignacion.AsignacionId }, creada);
    }

    /// <summary>Actualiza una asignacion. Requiere rol Admin.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] AsignacionRequestDto dto)
    {
        var asignacion = await _context.Asignaciones.FindAsync(id);
        if (asignacion is null)
        {
            return NotFound();
        }

        var existeEstudiante = await _context.Estudiantes.AnyAsync(e => e.EstudianteId == dto.EstudianteId);
        var existeCurso = await _context.Cursos.AnyAsync(c => c.CursoId == dto.CursoId);
        var existeDocente = await _context.Docentes.AnyAsync(d => d.DocenteId == dto.DocenteId);

        if (!existeEstudiante || !existeCurso || !existeDocente)
        {
            return BadRequest(new { mensaje = "El EstudianteId, CursoId o DocenteId no existen." });
        }

        asignacion.EstudianteId = dto.EstudianteId;
        asignacion.CursoId = dto.CursoId;
        asignacion.DocenteId = dto.DocenteId;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Elimina una asignacion. Requiere rol Admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var asignacion = await _context.Asignaciones.FindAsync(id);
        if (asignacion is null)
        {
            return NotFound();
        }

        _context.Asignaciones.Remove(asignacion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Task<AsignacionDto?> BuildDtoAsync(int id)
    {
        return _context.Asignaciones
            .AsNoTracking()
            .Where(a => a.AsignacionId == id)
            .Select(a => new AsignacionDto(
                a.AsignacionId,
                a.EstudianteId,
                a.Estudiante.Nombres + " " + a.Estudiante.Apellidos,
                a.CursoId,
                a.Curso.Nombre,
                a.DocenteId,
                a.Docente.Nombres + " " + a.Docente.Apellidos))
            .FirstOrDefaultAsync();
    }
}
