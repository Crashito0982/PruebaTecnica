namespace Gastos.Application.Dtos;

public sealed record UserDto(Guid Id, string Nombre, string Email, DateTime FechaCreacion, DateTime FechaActualizacion);
public sealed record UpdateMeRequest(string Nombre, string Email);
