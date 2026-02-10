
using Microsoft.EntityFrameworkCore;
using WebApplication2.Database;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("CoilDbContext")
                ?? BuildConnectionStringFromEnv();

            builder.Services.AddDbContext<CoilDbContext>(options =>
                options.UseNpgsql(connectionString));

            string BuildConnectionStringFromEnv()
            {
                var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
                var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
                var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "CoilStore";
                var username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
                var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres123";

                return $"Host={host};Port={port};Database={database};Username={username};Password={password};";
            }

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || bool.TryParse(Environment.GetEnvironmentVariable("RUN_MIGRATIONS"), out bool runMigrations) && runMigrations)
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CoilDbContext>();

                    // Проверяем, что БД доступна
                    if (dbContext.Database.CanConnect())
                    {
                        try
                        {
                            // миграции
                            dbContext.Database.Migrate();
                            Console.WriteLine("Миграции успешно применены");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при применении миграций: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Не удалось подключиться к БД для применения миграций");
                    }
                }
            }

            app.MapGet("/health", () =>
            {
                return Results.Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Environment = app.Environment.EnvironmentName
                });
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=StoreManagement}/{action=GetCoils}/{id?}");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
