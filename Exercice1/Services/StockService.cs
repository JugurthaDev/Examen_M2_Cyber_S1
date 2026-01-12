using Examen_Cyber_M2_S1.Services;

namespace Examen_Cyber_M2_S1.Services;

using Examen_Cyber_M2_S1.Models;

public sealed class StockService : IStockService
{
    private readonly object _lock = new();

    private readonly List<Product> _products = new()
    {
        new() { Id = 1, Name = "Clavier mécanique", Stock = 30, UnitPrice = 100.00m },
        new() { Id = 2, Name = "Souris gaming", Stock = 40, UnitPrice = 60.00m },
        new() { Id = 3, Name = "Écran gameur", Stock = 50, UnitPrice = 200.00m },
        new() { Id = 4, Name = "Casque audio", Stock = 60, UnitPrice = 90.00m },
        new() { Id = 5, Name = "Webcam", Stock = 70, UnitPrice = 50.00m },
    };

    public IReadOnlyList<Product> GetAllProducts()
    {
        lock (_lock)
        {
            return _products.Select(Clone).ToList();
        }
    }

    public Product? GetById(int id)
    {
        lock (_lock)
        {
            var p = _products.FirstOrDefault(x => x.Id == id);
            return p is null ? null : Clone(p);
        }
    }

    public bool TryReserve(int productId, int quantity, out string? error)
    {
        error = null;

        if (quantity <= 0)
        {
            error = "La quantité doit être supérieure à 0";
            return false;
        }

        lock (_lock)
        {
            var p = _products.FirstOrDefault(x => x.Id == productId);
            if (p is null)
            {
                error = $"Le produit avec l'identifiant {productId} n'existe pas";
                return false;
            }

            if (quantity > p.Stock)
            {
                error = $"Il ne reste que {p.Stock} exemplaire pour le produit {p.Name}";
                return false;
            }

            p.Stock -= quantity;
            return true;
        }
    }

    private static Product Clone(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Stock = p.Stock,
        UnitPrice = p.UnitPrice
    };
}