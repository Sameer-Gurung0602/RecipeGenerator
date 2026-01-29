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
            "http://localhost:3000"   // Create React App dev server
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

        // Only seed in Development environment
        if (app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
        {
            logger.LogInformation("Starting database seeding...");
            await DbSeeder.SeedAsync(context);
            logger.LogInformation("Database seeding completed successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding: {Message}", ex.Message);
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
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
