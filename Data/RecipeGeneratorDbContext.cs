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

        // DbSet properties for each table  
        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Instructions> Instructions { get; set; }
        public DbSet<DietaryRestrictions> DietaryRestrictions { get; set; }
        public DbSet<RecipeDietaryRestrictions> RecipeDietaryRestrictions { get; set; }
        public DbSet<RecipeIngredients> RecipeIngredients { get; set; }
        public DbSet<UserSavedRecipes> UserSavedRecipes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one: Recipe <-> Instructions (FK in Instructions table)
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Instructions)
                .WithOne(i => i.Recipe)
                .HasForeignKey<Instructions>(i => i.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure composite key for RecipeDietaryRestrictions (many-to-many join table)
            modelBuilder.Entity<RecipeDietaryRestrictions>()
                .HasKey(rd => new { rd.RecipeId, rd.DietaryRestrictionsId });

            modelBuilder.Entity<RecipeDietaryRestrictions>()
                .HasOne(rd => rd.Recipe)
                .WithMany(r => r.RecipeDietaryRestrictions)
                .HasForeignKey(rd => rd.RecipeId);
            
            modelBuilder.Entity<RecipeDietaryRestrictions>()
                .HasOne(rd => rd.DietaryRestrictions)
                .WithMany(d => d.RecipeDietaryRestrictions)
                .HasForeignKey(rd => rd.DietaryRestrictionsId);  // ✅ Fixed: Id instead of ID

            // Configure composite key for RecipeIngredients (many-to-many join table)
            modelBuilder.Entity<RecipeIngredients>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            modelBuilder.Entity<RecipeIngredients>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            modelBuilder.Entity<RecipeIngredients>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);

            // Configure composite key for UserSavedRecipes (many-to-many join table)
            modelBuilder.Entity<UserSavedRecipes>()
                .HasKey(us => new { us.UserId, us.RecipeId });

            modelBuilder.Entity<UserSavedRecipes>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSavedRecipes)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSavedRecipes>()
                .HasOne(us => us.Recipe)
                .WithMany(r => r.UserSavedRecipes)
                .HasForeignKey(us => us.RecipeId);
        }
    }
}