namespace Examen_Cyber_M2_S1.Models;

public sealed class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
}

