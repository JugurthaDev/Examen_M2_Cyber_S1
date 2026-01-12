namespace Examen_Cyber_M2_S1.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
    public decimal UnitPrice { get; set; }
}