namespace Gastos.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }

    public Guid UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }

    public ICollection<Expense> Gastos { get; set; } = new List<Expense>();
}
