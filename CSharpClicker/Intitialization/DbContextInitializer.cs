using CSharpClicker.Domain;
using CSharpClicker.Infrastructure.Implementations;
using Microsoft.EntityFrameworkCore;

namespace CSharpClicker.Intitialization;

public static class DbContextInitializer
{
    public static void InitializeDbContext(IServiceCollection services)
    {
        var dbFilePath = GetPathToDatabaseFile();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbFilePath}"));
    }

    public static void InitializeDataBase(AppDbContext dbContext)
    {
        dbContext.Database.Migrate();
        AddOrUpdateBoosts(dbContext);
    }

    private static void AddOrUpdateBoosts(AppDbContext dbContext)
    {
        // баланс настроен по принципу геометрической прогрессии.
        // цены растут, но и доходность новых зданий перекрывает старые.
        var boosts = new Boost[]
        {
            new()
            {
                Title = "Стальная кирка",
                Price = 15,
                Profit = 1,
                IsAuto = false,
                Image = GetImageBytes("pickaxe.png"),
            },
            new()
            {
                Title = "Крестьянин",
                Price = 100,
                Profit = 5,
                IsAuto = true,
                Image = GetImageBytes("peasant.png"),
            },
            new()
            {
                Title = "Шахта",
                Price = 1100,
                Profit = 50,
                IsAuto = true,
                Image = GetImageBytes("shaft.png"),
            },
            new()
            {
                Title = "Священник",
                Price = 12000,
                Profit = 350,
                IsAuto = true,
                Image = GetImageBytes("priest.png"),
            },
            new()
            {
                Title = "Динамит",
                Price = 130000,
                Profit = 2500,
                IsAuto = false,
                Image = GetImageBytes("explosion.png"),
            },
            new()
            {
                Title = "Слоник",
                Price = 1400000,
                Profit = 15000,
                IsAuto = true,
                Image = GetImageBytes("elephant.png"),
            },
            new()
            {
                Title = "Химическая обработка", 
                Price = 20000000, 
                Profit = 150000, 
                IsAuto = true,
                Image = GetImageBytes("chemistry.png"),
            },
        };

        foreach (var boost in boosts)
        {
            // проверяем, есть ли буст с таким названием
            var existingBoost = dbContext.Boosts.FirstOrDefault(b => b.Title == boost.Title);

            if (existingBoost == null)
            {
                dbContext.Boosts.Add(boost);
            }
            else
            {
                // если буст уже есть, обновляем ему цену и статы (на случай ребаланса)
                // но НЕ трогаем Id, чтобы не сломать связи с юзерами
                existingBoost.Price = boost.Price;
                existingBoost.Profit = boost.Profit;
                existingBoost.IsAuto = boost.IsAuto;
            }
        }

        dbContext.SaveChanges();

        static byte[] GetImageBytes(string imageName)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "resources", imageName);
            if (!File.Exists(filePath)) return Array.Empty<byte>();
            return File.ReadAllBytes(filePath);
        }
    }

    public static string GetPathToDatabaseFile()
    {
        var pathToLocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Directory.CreateDirectory(Path.Combine(pathToLocalApplicationData, "CSharpClicker"));

        var dbFilePath = Path.Combine(pathToLocalApplicationData, "CSharpClicker", "CSharpClicker.db");

        return dbFilePath;
    }
}