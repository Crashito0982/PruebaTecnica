using Gastos.Domain.Entities;

namespace Gastos.Infrastructure.Persistence;

public static class DevSeeder
{
    public static void Seed(GastosDbContext db)
    {
        var u1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var u2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Categorías demo (solo si no hay)
        if (!db.Categories.Any())
        {
            var comida1 = new Category { Id = Guid.NewGuid(), UsuarioId = u1, Nombre = "Comida", Descripcion = "Gastos de comida", FechaCreacion = DateTime.UtcNow};
            var transporte1 = new Category { Id = Guid.NewGuid(), UsuarioId = u1, Nombre = "Transporte", Descripcion = "Movilidad", FechaCreacion = DateTime.UtcNow};
            var servicios1 = new Category { Id = Guid.NewGuid(), UsuarioId = u1, Nombre = "Servicios", Descripcion = "Luz/agua/internet", FechaCreacion = DateTime.UtcNow};

            var comida2 = new Category { Id = Guid.NewGuid(), UsuarioId = u2, Nombre = "Comida", Descripcion = "Gastos de comida", FechaCreacion = DateTime.UtcNow};
            var ocio2 = new Category { Id = Guid.NewGuid(), UsuarioId = u2, Nombre = "Ocio", Descripcion = "Salidas", FechaCreacion = DateTime.UtcNow};

            db.Categories.AddRange(comida1, transporte1, servicios1, comida2, ocio2);
            db.SaveChanges();
        }

        // Gastos demo (solo si no hay)
        if (!db.Expenses.Any())
        {
            var catsU1 = db.Categories.Where(c => c.UsuarioId == u1).Select(c => c.Id).ToList();
            var rng = new Random(12345);

            // 30 gastos para u1 en fechas distintas
            for (int i = 0; i < 30; i++)
            {
                var catId = catsU1[rng.Next(catsU1.Count)];
                var monto = rng.Next(1000, 50000);
                var fecha = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-i));

                var desc = (i % 7 == 0) ? "Café Martinez" : $"Gasto demo #{i + 1}";

                db.Expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = u1,
                    CategoriaId = catId,
                    Monto = monto,
                    Fecha = fecha,
                    Descripcion = desc,
                    FechaCreacion = DateTime.UtcNow
                });
            }

            db.SaveChanges();
        }
    }
}
