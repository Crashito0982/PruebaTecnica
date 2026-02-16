namespace Gastos.Application.Dtos;

public sealed record ExpenseDto(
    Guid Id,
    decimal Monto,
    DateOnly Fecha,
    string? Descripcion,
    Guid CategoriaId,
    DateTime FechaCreacion
);

public sealed record CreateExpenseRequest(
    decimal Monto,
    DateOnly Fecha,
    string? Descripcion,
    Guid CategoriaId
);

public sealed record UpdateExpenseRequest(
    decimal Monto,
    DateOnly Fecha,
    string? Descripcion,
    Guid CategoriaId
);
