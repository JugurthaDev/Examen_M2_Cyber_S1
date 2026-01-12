namespace Examen_Cyber_M2_S1.Services;

using Examen_Cyber_M2_S1.Models;

public interface IStockService
{
    IReadOnlyList<Product> GetAllProducts();

    Product? GetById(int id);

    /// Réserve (décrémente) du stock. Retourne false si stock insuffisant.
    bool TryReserve(int productId, int quantity, out string? error);
}