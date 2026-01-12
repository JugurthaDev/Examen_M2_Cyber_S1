namespace Examen_Cyber_M2_S1.Services;

using Examen_Cyber_M2_S1.Models;
using Microsoft.EntityFrameworkCore;

public sealed class DbStockService : IStockService
{
    private readonly AppDbContext _db;

    public DbStockService(AppDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<Product> GetAllProducts()
    {
        return _db.Products.AsNoTracking().ToList();
    }

    public Product? GetById(int id)
    {
        return _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
    }

    public bool TryReserve(int productId, int quantity, out string? error)
    {
        error = null;
        if (quantity <= 0)
        {
            error = "La quantité doit être supérieure à 0";
            return false;
        }

        var p = _db.Products.FirstOrDefault(p => p.Id == productId);
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
        _db.SaveChanges();
        return true;
    }
}

