using Gastos.Application.Abstractions;
using Gastos.Application.Dtos;
using Gastos.Infrastructure.Persistence;
using Gastos.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gastos.Api.Controllers;

[ApiController]
[Route("categories")]
public class CategoriesController : ControllerBase
{
    private readonly GastosDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CategoriesController(GastosDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<CategoryDto>>> Get(CancellationToken ct)
    {
        var items = await _db.Categories.AsNoTracking()
            .Where(c => c.UsuarioId == _currentUser.UserId)
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new CategoryDto(c.Id, c.Nombre, c.Descripcion, c.FechaCreacion))
            .ToListAsync(ct);

        return items;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre))
            throw new BadRequestApiException("nombre es requerido.");

        var exists = await _db.Categories.AnyAsync(c =>
            c.UsuarioId == _currentUser.UserId && c.Nombre == req.Nombre, ct);

        if (exists) throw new ConflictApiException("Ya existe una categoría con ese nombre para este usuario.");

        var c = new Gastos.Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Nombre = req.Nombre.Trim(),
            Descripcion = req.Descripcion,
            UsuarioId = _currentUser.UserId,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Categories.Add(c);
        await _db.SaveChangesAsync(ct);

        var dto = new CategoryDto(c.Id, c.Nombre, c.Descripcion, c.FechaCreacion);
        return Created($"/categories/{c.Id}", dto);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest req, CancellationToken ct)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) throw new NotFoundApiException("Categoría no encontrada.");
        if (c.UsuarioId != _currentUser.UserId) throw new ForbiddenApiException("No puedes modificar categorías de otro usuario.");

        c.Nombre = req.Nombre.Trim();
        c.Descripcion = req.Descripcion;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Probable conflicto por índice único (UsuarioId, Nombre)
            throw new ConflictApiException("Ya existe una categoría con ese nombre para este usuario.");
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) throw new NotFoundApiException("Categoría no encontrada.");
        if (c.UsuarioId != _currentUser.UserId) throw new ForbiddenApiException("No puedes eliminar categorías de otro usuario.");

        var hasExpenses = await _db.Expenses.AnyAsync(e => e.CategoriaId == id, ct);
        if (hasExpenses) throw new ConflictApiException("No se puede eliminar la categoría porque tiene gastos asociados.");

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
