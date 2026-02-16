namespace Gastos.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string Email { get; set; } = default!;

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }
}
