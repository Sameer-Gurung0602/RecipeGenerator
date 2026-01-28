using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Models;

namespace RecipeGenerator.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(RecipeGeneratorDbContext context)
        {
            if (!context.Users.Any())
            {
                await SeedWithIdentityInsert(context, "Users", async () =>
                {
                    var users = await ReadFile<User>("SeedData/users.json");
                    context.Users.AddRange(users);
                });
            }

            if (!context.Recipes.Any())
            {
                await SeedWithIdentityInsert(context, "Recipes", async () =>
                {
                    var recipes = await ReadFile<Recipe>("SeedData/Recipes.json");
                    context.Recipes.AddRange(recipes);
                });
            }
            
            if(!context.Ingredients.Any())
            {
                await SeedWithIdentityInsert(context, "Ingredients", async () =>
                {
                    var ingredients = await ReadFile<Ingredient>("SeedData/Ingredients.json");
                    context.Ingredients.AddRange(ingredients);
                });
            }

            if (!context.Instructions.Any())
            {
                await SeedWithIdentityInsert(context, "Instructions", async () =>
                {
                    var instructions = await ReadFile<Instructions>("SeedData/Instructions.json");
                    context.Instructions.AddRange(instructions);
                });
            }
            
            if(!context.DietaryRestrictions.Any())
            {
                await SeedWithIdentityInsert(context, "DietaryRestrictions", async () =>
                {
                    var dietaryRestrictions = await ReadFile<DietaryRestrictions>("SeedData/DietaryRestrictions.json");
                    context.DietaryRestrictions.AddRange(dietaryRestrictions);
                });
            }

            // Seed junction tables
            await SeedUserRecipes(context);
            await SeedRecipeIngredients(context);
            await SeedRecipeDietaryRestrictions(context);
        }

        private static async Task SeedWithIdentityInsert(RecipeGeneratorDbContext context, string tableName, Func<Task> seedAction)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    // Check if we're using SQL Server
                    var isSqlServer = context.Database.IsSqlServer();
                    
                    if (isSqlServer)
                    {
                        // Enable IDENTITY_INSERT for SQL Server
                        await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON");
                    }
                    
                    await seedAction();
                    await context.SaveChangesAsync();
                    
                    if (isSqlServer)
                    {
                        // Disable IDENTITY_INSERT for SQL Server
                        await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF");
                    }
                    
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static async Task SeedUserRecipes(RecipeGeneratorDbContext context)
        {
            // Use SQL Server syntax for checking table existence
            var isSqlServer = context.Database.IsSqlServer();
            var checkQuery = isSqlServer
                ? "SELECT CASE WHEN EXISTS(SELECT 1 FROM [UserRecipes]) THEN 1 ELSE 0 END AS [Value]"
                : "SELECT CASE WHEN EXISTS(SELECT 1 FROM \"UserRecipes\") THEN 1 ELSE 0 END AS \"Value\"";
            
            if (await context.Database.SqlQueryRaw<int>(checkQuery).FirstOrDefaultAsync() == 0)
            {
                var userRecipes = await ReadFile<UserRecipeDto>("SeedData/UserSavedRecipes.json");
                
                // Use EF Core instead of raw SQL
                foreach (var ur in userRecipes)
                {
                    var user = await context.Users.FindAsync(ur.UserId);
                    var recipe = await context.Recipes.FindAsync(ur.RecipeId);
                    
                    if (user != null && recipe != null)
                    {
                        user.Recipes ??= new List<Recipe>();
                        user.Recipes.Add(recipe);
                    }
                }
                
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedRecipeIngredients(RecipeGeneratorDbContext context)
        {
            // Use SQL Server syntax for checking table existence
            var isSqlServer = context.Database.IsSqlServer();
            var checkQuery = isSqlServer
                ? "SELECT CASE WHEN EXISTS(SELECT 1 FROM [RecipeIngredients]) THEN 1 ELSE 0 END AS [Value]"
                : "SELECT CASE WHEN EXISTS(SELECT 1 FROM \"RecipeIngredients\") THEN 1 ELSE 0 END AS \"Value\"";
            
            if (await context.Database.SqlQueryRaw<int>(checkQuery).FirstOrDefaultAsync() == 0)
            {
                var recipeIngredients = await ReadFile<RecipeIngredientDto>("SeedData/RecipeIngredients.json");
                
                foreach (var ri in recipeIngredients)
                {
                    var recipe = await context.Recipes.FindAsync(ri.RecipeId);
                    var ingredient = await context.Ingredients.FindAsync(ri.IngredientId);
                    
                    if (recipe != null && ingredient != null)
                    {
                        recipe.Ingredients ??= new List<Ingredient>();
                        recipe.Ingredients.Add(ingredient);
                    }
                }
                
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedRecipeDietaryRestrictions(RecipeGeneratorDbContext context)
        {
            // Use SQL Server syntax for checking table existence
            var isSqlServer = context.Database.IsSqlServer();
            var checkQuery = isSqlServer
                ? "SELECT CASE WHEN EXISTS(SELECT 1 FROM [RecipeDietaryRestrictions]) THEN 1 ELSE 0 END AS [Value]"
                : "SELECT CASE WHEN EXISTS(SELECT 1 FROM \"RecipeDietaryRestrictions\") THEN 1 ELSE 0 END AS \"Value\"";
            
            if (await context.Database.SqlQueryRaw<int>(checkQuery).FirstOrDefaultAsync() == 0)
            {
                var recipeDietary = await ReadFile<RecipeDietaryRestrictionDto>("SeedData/RecipeDietaryRestrictions.json");
                
                foreach (var rd in recipeDietary)
                {
                    var recipe = await context.Recipes.FindAsync(rd.RecipeId);
                    var dietary = await context.DietaryRestrictions.FindAsync(rd.DietaryRestrictionsId);
                    
                    if (recipe != null && dietary != null)
                    {
                        recipe.DietaryRestrictions ??= new List<DietaryRestrictions>();
                        recipe.DietaryRestrictions.Add(dietary);
                    }
                }
                
                await context.SaveChangesAsync();
            }
        }

        public static async Task<List<T>> ReadFile<T>(string path)
        {
            var data = await File.ReadAllTextAsync(path);
            var list = JsonSerializer.Deserialize<List<T>>(data);
            return list ?? new List<T>();
        }
    }

    // DTOs for junction table seeding
    public class UserRecipeDto
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public DateTime? SavedAt { get; set; }
    }

    public class RecipeIngredientDto
    {
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
    }

    public class RecipeDietaryRestrictionDto
    {
        public int RecipeId { get; set; }
        public int DietaryRestrictionsId { get; set; }
    }
}