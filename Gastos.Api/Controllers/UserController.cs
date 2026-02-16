using Gastos.Application.Abstractions;
using Gastos.Application.Dtos;
using Gastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gastos.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly GastosDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UsersController(GastosDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var u = await _db.Users.AsNoTracking()
            .FirstAsync(x => x.Id == _currentUser.UserId, ct);

        return new UserDto(u.Id, u.Nombre, u.Email, u.FechaCreacion, u.FechaActualizacion);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest req, CancellationToken ct)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == _currentUser.UserId, ct);

        u.Nombre = req.Nombre;
        u.Email = req.Email;
        u.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
