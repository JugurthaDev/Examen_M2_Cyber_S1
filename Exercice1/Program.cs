namespace Examen_Cyber_M2_S1;

using Examen_Cyber_M2_S1.Services;
using Examen_Cyber_M2_S1.Models;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("ExamenCyberDb"));

        builder.Services.AddScoped<IStockService, DbStockService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Products.AddRange(new Product { Id = 1, Name = "Clavier mécanique", Stock = 30, UnitPrice = 100m },
                                 new Product { Id = 2, Name = "Souris gaming", Stock = 40, UnitPrice = 60m },
                                 new Product { Id = 3, Name = "Écran gameur", Stock = 50, UnitPrice = 200m },
                                 new Product { Id = 4, Name = "Casque audio", Stock = 60, UnitPrice = 90m },
                                 new Product { Id = 5, Name = "Webcam", Stock = 70, UnitPrice = 50m });

            db.PromoCodes.AddRange(new PromoCode { Id = 1, Code = "DISCOUNT20", Percentage = 20m },
                                   new PromoCode { Id = 2, Code = "DISCOUNT10", Percentage = 10m });

            db.SaveChanges();
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        app.Run();
    }
}