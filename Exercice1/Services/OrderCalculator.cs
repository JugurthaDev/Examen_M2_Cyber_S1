using Examen_Cyber_M2_S1.Services;

namespace Examen_Cyber_M2_S1.Services;

using Examen_Cyber_M2_S1.Models;

public static class OrderCalculator
{
    public static (OrderResponse? Response, List<string> Errors) TryCreateOrder(
        IStockService stockService,
        CreateOrderRequest request)
    {
        var errors = new List<string>();

        if (request.Products.Count == 0)
        {
            errors.Add("La commande doit contenir au moins un produit");
            return (null, errors);
        }

        // Validation produits + lecture infos nécessaires
        var lines = new List<(Product Product, int Quantity)>();
        foreach (var line in request.Products)
        {
            var product = stockService.GetById(line.Id);
            if (product is null)
            {
                errors.Add($"Le produit avec l'identifiant {line.Id} n'existe pas");
                continue;
            }

            if (line.Quantity <= 0)
            {
                errors.Add("La quantité doit être supérieure à 0");
                continue;
            }

            lines.Add((product, line.Quantity));
        }

        // Si erreurs de validation on ne touche pas au stock
        if (errors.Count > 0)
            return (null, errors);

        // Vérification stock sans réserver encore -> utilise TryReserve
        foreach (var (product, quantity) in lines)
        {
            if (quantity > product.Stock)
                errors.Add($"Il ne reste que {product.Stock} exemplaire pour le produit {product.Name}");
        }

        // Total avant remises 
        var totalBeforeDiscounts = lines.Sum(l => l.Product.UnitPrice * l.Quantity);

        // Promo validation
        var promoCode = request.PromoCode;
        decimal promoPercent = 0m;
        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            promoCode = promoCode.Trim();

            if (promoCode is not ("DISCOUNT20" or "DISCOUNT10"))
            {
                errors.Add("Le code promo est invalide");
            }
            else if (totalBeforeDiscounts <= 50m)
            {
                errors.Add("Les codes promos ne sont valables qu'a partir de 50e d'achat");
            }
            else
            {
                promoPercent = promoCode == "DISCOUNT20" ? 20m : 10m;
            }
        }

        if (errors.Count > 0)
            return (null, errors);

        // Réservation stock si l'une echoue on collecte l'erreur et on stop
        foreach (var (product, quantity) in lines)
        {
            if (!stockService.TryReserve(product.Id, quantity, out var reserveError))
            {
                if (!string.IsNullOrWhiteSpace(reserveError))
                    errors.Add(reserveError);
            }
        }

        if (errors.Count > 0)
            return (null, errors);

        // Calcul lignes + remise quantité >5 => -10% sur la ligne
        var response = new OrderResponse();
        foreach (var (product, quantity) in lines)
        {
            var lineTotal = product.UnitPrice * quantity;
            if (quantity > 5)
                lineTotal *= 0.90m;

            response.Products.Add(new OrderProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = quantity,
                PricePerUnit = product.UnitPrice,
                Total = Round2(lineTotal)
            });
        }

        var subtotalAfterLineDiscounts = response.Products.Sum(p => p.Total);

        // Remise order 5% si total > 100 après remise quantité
        decimal orderPercent = subtotalAfterLineDiscounts > 100m ? 5m : 0m;
        if (orderPercent > 0m)
        {
            response.Discounts.Add(new DiscountResponse
            {
                Type = "order",
                Value = orderPercent
            });
        }

        if (promoPercent > 0m)
        {
            response.Discounts.Add(new DiscountResponse
            {
                Type = "promo",
                Value = promoPercent
            });
        }

        var totalPercent = orderPercent + promoPercent;
        var finalTotal = subtotalAfterLineDiscounts * ((100m - totalPercent) / 100m);
        response.Total = Round2(finalTotal);

        return (response, errors);
    }

    private static decimal Round2(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
