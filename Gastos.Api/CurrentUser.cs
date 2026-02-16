using Gastos.Application.Abstractions;

namespace Gastos.Api.Auth;

public sealed class CurrentUser : ICurrentUser
{
    private Guid _userId;

    public Guid UserId =>
        _userId == Guid.Empty
            ? throw new InvalidOperationException("UserId no inicializado")
            : _userId;

    public void Set(Guid userId) => _userId = userId;
}
