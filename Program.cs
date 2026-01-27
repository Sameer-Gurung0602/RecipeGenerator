using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }); // For API

// Register DbContext with SQL Server
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible to tests
public partial class Program { }
