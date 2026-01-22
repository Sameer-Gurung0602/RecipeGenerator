using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
=======
// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
      }); // For API

>>>>>>> 365be23d219f1d5e590b421f6163a486e826d047
// Register DbContext with SQL Server
builder.Services.AddDbContext<RecipeGeneratorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// register RecipeService
builder.Services.AddScoped<RecipeService>();

builder.Services.AddControllers();

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
