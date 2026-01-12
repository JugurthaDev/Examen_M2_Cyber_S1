namespace Examen_Cyber_M2_S1.Models;

using System.Text.Json.Serialization;

public sealed class CreateOrderRequest
{
    public List<CreateOrderProductRequest> Products { get; set; } = new();

    [JsonPropertyName("promo_code")]
    public string? PromoCode { get; set; }
}

public sealed class CreateOrderProductRequest
{
    public int Id { get; set; }
    public int Quantity { get; set; }
}

public sealed class OrderResponse
{
    public List<OrderProductResponse> Products { get; set; } = new();
    public List<DiscountResponse> Discounts { get; set; } = new();
    public decimal Total { get; set; }
}

public sealed class OrderProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }

    [JsonPropertyName("price_per_unit")]
    public decimal PricePerUnit { get; set; }

    public decimal Total { get; set; }
}

public sealed class DiscountResponse
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public sealed class ErrorResponse
{
    public List<string> Errors { get; set; } = new();
}
