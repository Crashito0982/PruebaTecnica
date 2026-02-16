namespace Gastos.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
}
