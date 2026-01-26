using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeGenerator.Data;
using RecipeGenerator.Test.Helpers;

namespace RecipeGenerator.Test
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RecipeGeneratorDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using SQL Server test database
                services.AddDbContext<RecipeGeneratorDbContext>(options =>
                {
                    options.UseSqlServer(
                        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RecipeGenerator-test;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<RecipeGeneratorDbContext>();

                    Console.WriteLine("🔧 Setting up test database...");
                    
                    // Clean slate for each test run
                    if (db.Database.EnsureCreated())
                    {
                        db.Database.EnsureDeleted();
                    }
                    
                    // Use Migrate instead of EnsureCreated to apply proper migrations
                    db.Database.Migrate();
                    Console.WriteLine("✅ Database migrated");

                    // Seed test data - use direct await instead of Task.Run
                    Console.WriteLine("📦 Seeding test data...");
                    TestDataSeeder.SeedFromTestData(db).GetAwaiter().GetResult();
                    
                    // Clear change tracker after seeding
                    db.ChangeTracker.Clear();
                    Console.WriteLine("✅ Seeding completed");

                    // Verify data was inserted
                    var userCount = db.Users.Count();
                    Console.WriteLine($"📊 Users in database: {userCount}");
                }
            });

            builder.UseEnvironment("Testing");
        }
    }
}