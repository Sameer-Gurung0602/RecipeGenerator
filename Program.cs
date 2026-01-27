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
            "enter hosted front end URL here"  // Update when you deploy React app
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

// Register DbContext with SQL Server for testing
builder.Services.AddDbContext<RecipeGeneratorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// register RecipeService
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<FavouritesService>();

var app = builder.Build();

// Seed the database (skip during testing)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<RecipeGeneratorDbContext>();
        await DbSeeder.SeedAsync(context);
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
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

// Make Program accessible to tests
public partial class Program { }
