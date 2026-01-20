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

            // Configure composite key for RecipeDietaryRestrictions (many-to-many join table)
            modelBuilder.Entity<RecipeDietaryRestrictions>()
                .HasKey(rd => new { rd.RecipeID, rd.DietaryRestrictionID });

            modelBuilder.Entity<RecipeDietaryRestrictions>()
                .HasOne(rd => rd.Recipe)
                .WithMany(r => r.RecipeDietaryRestrictions)
                .HasForeignKey(rd => rd.RecipeID);
            //One Recipe can have many dietary restrictions
            
            modelBuilder.Entity<RecipeDietaryRestrictions>()
                .HasOne(rd => rd.DietaryRestrictions)
                .WithMany(d => d.RecipeDietaryRestrictions)
                .HasForeignKey(rd => rd.DietaryRestrictionID);

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

            // Configure one-to-one: Recipe <-> Instructions
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Instructions)           // Recipe has ONE Instructions
                .WithOne(i => i.Recipe)                // Instructions has ONE Recipe
                .HasForeignKey<Instructions>(i => i.RecipeId)  // Foreign key is in Instructions table
                .OnDelete(DeleteBehavior.Cascade);     // If recipe deleted, delete instructions too
        }
    }
}