using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeGenerator.Data;
using RecipeGenerator.Test.Helpers;

namespace RecipeGenerator.Test
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
    {
        private bool _disposed = false;

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

                    try
                    {
                        Console.WriteLine("🔧 Setting up test database...");
                        
                        // Clean slate for each test run
                        try
                        {
                            Console.WriteLine("🗑️ Attempting to delete existing database...");
                            db.Database.EnsureDeleted();
                            Console.WriteLine("✅ Database deleted");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Could not delete database: {ex.Message}");
                        }
                        
                        // Use EnsureCreated for tests
                        db.Database.EnsureCreated();
                        Console.WriteLine("✅ Database created");

                        // Seed test data
                        Console.WriteLine("📦 Seeding test data...");
                        TestDataSeeder.SeedFromTestData(db).GetAwaiter().GetResult();
                        
                        // Clear change tracker after seeding
                        db.ChangeTracker.Clear();
                        Console.WriteLine("✅ Seeding completed");

                        // Verify data was inserted
                        var userCount = db.Users.Count();
                        Console.WriteLine($"📊 Users in database: {userCount}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error setting up database: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        throw;
                    }
                }
            });

            builder.UseEnvironment("Testing");
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Clean up database on dispose
                    try
                    {
                        using var scope = Services.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<RecipeGeneratorDbContext>();
                        db.Database.EnsureDeleted();
                        Console.WriteLine("🧹 Test database cleaned up");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not clean up database: {ex.Message}");
                    }
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }

    // Collection definition to prevent parallel test execution
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}