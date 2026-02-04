using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",  // Vite dev server
            "https://recipegeneratorkpmg.netlify.app"   // Create React App dev server
        )
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }); // For API

// Register DbContext with SQL Server
builder.Services.AddDbContext<RecipeGeneratorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnector")));

// Register services
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<FavouritesService>();

var app = builder.Build();

// Migrate and optionally seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RecipeGeneratorDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database migration...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully.");

        // Seed if database is empty (works in all environments)
        if (!context.Recipes.Any())
        {
            logger.LogInformation("Database is empty. Starting database seeding...");
            await DbSeeder.SeedAsync(context);
            logger.LogInformation("Database seeding completed successfully.");
        }
        else
        {
            logger.LogInformation("Database already contains data. Skipping seeding.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding: {Message}", ex.Message);
        // In production, you might want to rethrow to prevent the app from starting with a broken database
        // throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
