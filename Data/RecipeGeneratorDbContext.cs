using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Models;

namespace RecipeGenerator.Data
{
    public class RecipeGeneratorDbContext : DbContext
    {
        public RecipeGeneratorDbContext(DbContextOptions<RecipeGeneratorDbContext> options)
            : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Instructions> Instructions { get; set; }
        public DbSet<DietaryRestrictions> DietaryRestrictions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Recipes)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRecipes",
                    j => j.HasOne<Recipe>().WithMany().HasForeignKey("RecipeId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"));

            modelBuilder.Entity<Recipe>()
                .HasMany(r => r.Ingredients)
                .WithMany(i => i.Recipes)
                .UsingEntity<Dictionary<string, object>>(
                    "RecipeIngredients",
                    j => j.HasOne<Ingredient>().WithMany().HasForeignKey("IngredientId"),
                    j => j.HasOne<Recipe>().WithMany().HasForeignKey("RecipeId"));

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Instructions)
                .WithOne(i => i.Recipe)
                .HasForeignKey<Instructions>(i => i.RecipeId);  

            modelBuilder.Entity<Recipe>()
                .HasMany(r => r.DietaryRestrictions)
                .WithMany(d => d.Recipes)
                .UsingEntity<Dictionary<string, object>>(
                    "RecipeDietaryRestrictions",
                    j => j.HasOne<DietaryRestrictions>().WithMany().HasForeignKey("DietaryRestrictionsId"),
                    j => j.HasOne<Recipe>().WithMany().HasForeignKey("RecipeId"));  

            // SQL Server timestamp default
            modelBuilder.Entity<Recipe>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");  // Changed from NOW() to GETDATE() for SQL Server
        }
    }
}