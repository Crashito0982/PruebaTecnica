namespace Gastos.Application.Dtos;

public sealed record CategoryDto(Guid Id, string Nombre, string? Descripcion, DateTime FechaCreacion);

public sealed record CreateCategoryRequest(string Nombre, string? Descripcion);

public sealed record UpdateCategoryRequest(string Nombre, string? Descripcion);
