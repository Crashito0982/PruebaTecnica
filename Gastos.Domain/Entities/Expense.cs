namespace Gastos.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public decimal Monto { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Descripcion { get; set; }

    public Guid CategoriaId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
}
