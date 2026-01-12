namespace Examen_Cyber_M2_S1.Tests;

using Examen_Cyber_M2_S1.Services;
using Examen_Cyber_M2_S1.Models;

public class OrderCalculatorTests
{
    private static IStockService CreateStock() => new StockService();

    [Fact]
    public void CreateOrder_Valid_NoDiscounts_TotalIsSum()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 2, Quantity = 1 } // 60
            }
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Empty(errors);
        Assert.NotNull(resp);
        Assert.Equal(60.00m, resp!.Total);
        Assert.Empty(resp.Discounts);
        Assert.Single(resp.Products);
        Assert.Equal(60.00m, resp.Products[0].Total);
    }

    [Fact]
    public void CreateOrder_ProductQuantityOver5_Applies10PercentLineDiscount()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 2, Quantity = 6 } // 6*60=360 -> -10% => 324, puis total>100 => -5% => 307.80
            }
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Empty(errors);
        Assert.NotNull(resp);
        Assert.Equal(307.80m, resp!.Total);
    }

    [Fact]
    public void CreateOrder_TotalOver100_AppliesOrderDiscount5Percent()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 1, Quantity = 2 } // 2*100 = 200 => order -5% => 190
            }
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Empty(errors);
        Assert.NotNull(resp);
        Assert.Contains(resp!.Discounts, d => d.Type == "order" && d.Value == 5m);
        Assert.Equal(190.00m, resp.Total);
    }

    [Fact]
    public void CreateOrder_PromoInvalid_Returns400ErrorsShapeMessage()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 1, Quantity = 1 }
            },
            PromoCode = "NOPE"
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Null(resp);
        Assert.Contains(errors, e => e.Contains("code promo", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateOrder_PromoRequiresMoreThan50BeforeDiscounts()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 5, Quantity = 1 } // 50
            },
            PromoCode = "DISCOUNT10"
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Null(resp);
        Assert.Contains(errors, e => e.Contains("codes promos", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateOrder_PromoAndOrderDiscount_AreAdditive()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 1, Quantity = 2 } // 200
            },
            PromoCode = "DISCOUNT10" // + order 5 => total percent 15 => 170
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Empty(errors);
        Assert.NotNull(resp);
        Assert.Contains(resp!.Discounts, d => d.Type == "order" && d.Value == 5m);
        Assert.Contains(resp.Discounts, d => d.Type == "promo" && d.Value == 10m);
        Assert.Equal(170.00m, resp.Total);
    }

    [Fact]
    public void CreateOrder_UnknownProduct_ReturnsError()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 111, Quantity = 1 }
            }
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Null(resp);
        Assert.Contains(errors, e => e.Contains("identifiant 111", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateOrder_InsufficientStock_ReturnsError()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 1, Quantity = 999 }
            }
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Null(resp);
        Assert.Contains(errors, e => e.StartsWith("Il ne reste que ", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateOrder_MultipleErrors_AreAllReturned()
    {
        var stock = CreateStock();
        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 999, Quantity = 1 },
                new() { Id = 1, Quantity = 0 }
            },
            PromoCode = "INVALID"
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Null(resp);
        Assert.Contains(errors, e => e.Contains("identifiant 999", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("quantit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateOrder_Valid_ReservesStockInMemory()
    {
        var stock = CreateStock();

        var before = stock.GetById(1)!.Stock;

        var req = new CreateOrderRequest
        {
            Products = new()
            {
                new() { Id = 1, Quantity = 3 }
            }
        };

        var (resp, errors) = OrderCalculator.TryCreateOrder(stock, req);

        Assert.Empty(errors);
        Assert.NotNull(resp);

        var after = stock.GetById(1)!.Stock;
        Assert.Equal(before - 3, after);
    }
}
