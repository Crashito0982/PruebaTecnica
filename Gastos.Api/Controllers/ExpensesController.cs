using Gastos.Application.Abstractions;
using Gastos.Application.Dtos;
using Gastos.Api.Middleware;
using Gastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gastos.Api.Controllers;

[ApiController]
[Route("expenses")]
public class ExpensesController : ControllerBase
{
    private readonly GastosDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ExpensesController(GastosDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? sortBy = "fecha",
        [FromQuery] string? sortOrder = "desc",
        CancellationToken ct = default)
    {
        if (page < 1) throw new BadRequestApiException("page debe ser >= 1.");
        if (pageSize < 1) throw new BadRequestApiException("pageSize debe ser >= 1.");
        if (pageSize > 100) throw new BadRequestApiException("pageSize no puede ser mayor a 100.");

        sortBy = (sortBy ?? "fecha").Trim().ToLowerInvariant();
        sortOrder = (sortOrder ?? "desc").Trim().ToLowerInvariant();

        if (sortBy is not ("monto" or "fecha"))
            throw new BadRequestApiException("sortBy debe ser 'monto' o 'fecha'.");

        if (sortOrder is not ("asc" or "desc"))
            throw new BadRequestApiException("sortOrder debe ser 'asc' o 'desc'.");

        var q = _db.Expenses.AsNoTracking()
            .Where(e => e.UsuarioId == _currentUser.UserId);

        if (categoryId is not null)
            q = q.Where(e => e.CategoriaId == categoryId.Value);

        // Búsqueda: ignorar mayúsculas y acentos (SQL Server collation CI_AI)
        if (!string.IsNullOrWhiteSpace(search))
        {
            const string CI_AI = "Latin1_General_100_CI_AI";
            var term = search.Trim();

            q = q.Where(e =>
                e.Descripcion != null &&
                EF.Functions.Like(
                    EF.Functions.Collate(e.Descripcion, CI_AI),
                    $"%{term}%"
                )
            );
        }

        // Orden
        q = (sortBy, sortOrder) switch
        {
            ("monto", "asc") => q.OrderBy(e => e.Monto).ThenBy(e => e.Fecha),
            ("monto", "desc") => q.OrderByDescending(e => e.Monto).ThenByDescending(e => e.Fecha),
            ("fecha", "asc") => q.OrderBy(e => e.Fecha).ThenBy(e => e.Monto),
            _ => q.OrderByDescending(e => e.Fecha).ThenByDescending(e => e.Monto)
        };

        var totalItems = await q.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExpenseDto(
                e.Id,
                e.Monto,
                e.Fecha,
                e.Descripcion,
                e.CategoriaId,
                e.FechaCreacion
            ))
            .ToListAsync(ct);

        var result = new PagedResult<ExpenseDto>(
            Page: page,
            PageSize: pageSize,
            TotalItems: totalItems,
            TotalPages: totalPages,
            Items: items
        );

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseRequest req, CancellationToken ct)
    {
        if (req.Monto <= 0) throw new BadRequestApiException("Monto debe ser mayor a 0.");

        // Validar que la categoira exista y sea del usuario actual
        var category = await _db.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.CategoriaId, ct);

        if (category is null) throw new NotFoundApiException("Categoría no encontrada.");
        if (category.UsuarioId != _currentUser.UserId) throw new ForbiddenApiException("No puedes usar categorías de otro usuario.");

        var e = new Gastos.Domain.Entities.Expense
        {
            Id = Guid.NewGuid(),
            Monto = req.Monto,
            Fecha = req.Fecha,
            Descripcion = req.Descripcion,
            CategoriaId = req.CategoriaId,
            UsuarioId = _currentUser.UserId,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Expenses.Add(e);
        await _db.SaveChangesAsync(ct);

        var dto = new ExpenseDto(e.Id, e.Monto, e.Fecha, e.Descripcion, e.CategoriaId, e.FechaCreacion);
        return Created($"/expenses/{e.Id}", dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseRequest req, CancellationToken ct)
    {
        if (req.Monto <= 0) throw new BadRequestApiException("Monto debe ser mayor a 0.");

        var e = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) throw new NotFoundApiException("Gasto no encontrado.");
        if (e.UsuarioId != _currentUser.UserId) throw new ForbiddenApiException("No puedes modificar gastos de otro usuario.");

        // Validar categoria
        var category = await _db.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.CategoriaId, ct);

        if (category is null) throw new NotFoundApiException("Categoría no encontrada.");
        if (category.UsuarioId != _currentUser.UserId) throw new ForbiddenApiException("No puedes usar categorías de otro usuario.");

        e.Monto = req.Monto;
        e.Fecha = req.Fecha;
        e.Descripcion = req.Descripcion;
        e.CategoriaId = req.CategoriaId;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var e = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) throw new NotFoundApiException("Gasto no encontrado.");
        if (e.UsuarioId != _currentUser.UserId) throw new ForbiddenApiException("No puedes eliminar gastos de otro usuario.");

        _db.Expenses.Remove(e);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
