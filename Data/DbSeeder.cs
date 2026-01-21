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
                    await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {tableName} ON");
                    await seedAction();
                    await context.SaveChangesAsync();
                    await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {tableName} OFF");
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
            var tableName = "UserRecipes";
            var hasData = await context.Database
                .SqlQueryRaw<int>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM {tableName}) THEN 1 ELSE 0 END AS int) AS Value")
                .FirstOrDefaultAsync();
            
            if (hasData == 0)
            {
                var userRecipes = await ReadFile<UserRecipeDto>("SeedData/UserSavedRecipes.json");
                
                foreach (var ur in userRecipes)
                {
                    await context.Database.ExecuteSqlRawAsync(
                        $"INSERT INTO {tableName} (UserId, RecipeId) VALUES ({ur.UserId}, {ur.RecipeId})");
                }
            }
        }

        private static async Task SeedRecipeIngredients(RecipeGeneratorDbContext context)
        {
            var tableName = "RecipeIngredients";
            var hasData = await context.Database
                .SqlQueryRaw<int>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM {tableName}) THEN 1 ELSE 0 END AS int) AS Value")
                .FirstOrDefaultAsync();
            
            if (hasData == 0)
            {
                var recipeIngredients = await ReadFile<RecipeIngredientDto>("SeedData/RecipeIngredients.json");
                
                foreach (var ri in recipeIngredients)
                {
                    await context.Database.ExecuteSqlRawAsync(
                        $"INSERT INTO {tableName} (RecipeId, IngredientId) VALUES ({ri.RecipeId}, {ri.IngredientId})");
                }
            }
        }

        private static async Task SeedRecipeDietaryRestrictions(RecipeGeneratorDbContext context)
        {
            var tableName = "RecipeDietaryRestrictions";
            var hasData = await context.Database
                .SqlQueryRaw<int>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM {tableName}) THEN 1 ELSE 0 END AS int) AS Value")
                .FirstOrDefaultAsync();
            
            if (hasData == 0)
            {
                var recipeDietary = await ReadFile<RecipeDietaryRestrictionDto>("SeedData/RecipeDietaryRestrictions.json");
                
                foreach (var rd in recipeDietary)
                {
                    await context.Database.ExecuteSqlRawAsync(
                        $"INSERT INTO {tableName} (RecipeId, DietaryRestrictionsId) VALUES ({rd.RecipeId}, {rd.DietaryRestrictionsId})");
                }
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