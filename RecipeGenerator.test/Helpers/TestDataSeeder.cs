using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.Models;

namespace RecipeGenerator.Test.Helpers
{
    public static class TestDataSeeder
    {
        public static async Task SeedFromTestData(RecipeGeneratorDbContext context)
        {
            // Seed Users with IDENTITY_INSERT
            if (!context.Users.Any())
            {
                Console.WriteLine("Seeding Users...");
                await SeedWithIdentityInsert(context, "Users", async () =>
                {
                    var users = await ReadFile<User>("test/Users.json");
                    context.Users.AddRange(users);
                });
                // Don't count here - it can cause tracking issues
                Console.WriteLine($"✅ Users seeded");
            }

            // Seed Ingredients with IDENTITY_INSERT
            if (!context.Ingredients.Any())
            {
                Console.WriteLine("Seeding Ingredients...");
                await SeedWithIdentityInsert(context, "Ingredients", async () =>
                {
                    var ingredients = await ReadFile<Ingredient>("test/Ingredients.json");
                    context.Ingredients.AddRange(ingredients);
                });
                Console.WriteLine($"✅ Ingredients seeded");
            }

            // Seed DietaryRestrictions with IDENTITY_INSERT
            if (!context.DietaryRestrictions.Any())
            {
                Console.WriteLine("Seeding DietaryRestrictions...");
                await SeedWithIdentityInsert(context, "DietaryRestrictions", async () =>
                {
                    var dietaryRestrictions = await ReadFile<DietaryRestrictions>("test/DietaryRestrictions.json");
                    context.DietaryRestrictions.AddRange(dietaryRestrictions);
                });
                Console.WriteLine($"✅ DietaryRestrictions seeded");
            }

            // Seed Recipes with IDENTITY_INSERT
            if (!context.Recipes.Any())
            {
                Console.WriteLine("Seeding Recipes...");
                await SeedWithIdentityInsert(context, "Recipes", async () =>
                {
                    var recipes = await ReadFile<Recipe>("test/Recipes.json");
                    context.Recipes.AddRange(recipes);
                });
                Console.WriteLine($"✅ Recipes seeded");
            }

            // Seed Instructions with IDENTITY_INSERT
            if (!context.Instructions.Any())
            {
                Console.WriteLine("Seeding Instructions...");
                await SeedWithIdentityInsert(context, "Instructions", async () =>
                {
                    var instructions = await ReadFile<Instructions>("test/Instructions.json");
                    context.Instructions.AddRange(instructions);
                });
                Console.WriteLine($"✅ Instructions seeded");
            }

            // Seed junction tables
            await SeedUserRecipes(context);
            await SeedRecipeIngredients(context);
            await SeedRecipeDietaryRestrictions(context);
        }

        private static async Task SeedWithIdentityInsert(RecipeGeneratorDbContext context, string tableName, Func<Task> seedAction)
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            
            if (!wasOpen)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine($"  Setting IDENTITY_INSERT ON for {tableName}");
#pragma warning disable EF1002
                    await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON");
#pragma warning restore EF1002

                    await seedAction();
                    
                    Console.WriteLine($"  Saving changes for {tableName}");
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine($"  Setting IDENTITY_INSERT OFF for {tableName}");
#pragma warning disable EF1002
                    await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF");
#pragma warning restore EF1002

                    await transaction.CommitAsync();
                    
                    // Clear change tracker after commit to prevent tracking conflicts
                     
                    Console.WriteLine($"  Transaction committed for {tableName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ Error seeding {tableName}: {ex.Message}");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static async Task SeedUserRecipes(RecipeGeneratorDbContext context)
        {
            var tableName = "UserRecipes";
            
            Console.WriteLine($"Seeding {tableName}...");
            
#pragma warning disable EF1002 // Risk of SQL injection - tableName is controlled internally
            var hasData = await context.Database
                .SqlQueryRaw<int>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [{tableName}]) THEN 1 ELSE 0 END AS int) AS Value")
                .FirstOrDefaultAsync();
#pragma warning restore EF1002

            if (hasData == 0)
            {
                var userRecipes = await ReadFile<UserRecipeDto>("test/UserSavedRecipes.json");
                Console.WriteLine($"  Inserting {userRecipes.Count} user-recipe relationships");

                foreach (var ur in userRecipes)
                {
                    await context.Database.ExecuteSqlAsync(
                        $"INSERT INTO [UserRecipes] (UserId, RecipeId) VALUES ({ur.UserId}, {ur.RecipeId})");
                }
                Console.WriteLine($"✅ {tableName} seeded");
            }
        }

        private static async Task SeedRecipeIngredients(RecipeGeneratorDbContext context)
        {
            var tableName = "RecipeIngredients";
            
            Console.WriteLine($"Seeding {tableName}...");
            
#pragma warning disable EF1002 // Risk of SQL injection - tableName is controlled internally
            var hasData = await context.Database
                .SqlQueryRaw<int>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [{tableName}]) THEN 1 ELSE 0 END AS int) AS Value")
                .FirstOrDefaultAsync();
#pragma warning restore EF1002

            if (hasData == 0)
            {
                var recipeIngredients = await ReadFile<RecipeIngredientDto>("test/RecipeIngredients.json");
                Console.WriteLine($"  Inserting {recipeIngredients.Count} recipe-ingredient relationships");

                foreach (var ri in recipeIngredients)
                {
                    await context.Database.ExecuteSqlAsync(
                        $"INSERT INTO [RecipeIngredients] (RecipeId, IngredientId) VALUES ({ri.RecipeId}, {ri.IngredientId})");
                }
                Console.WriteLine($"✅ {tableName} seeded");
            }
        }

        private static async Task SeedRecipeDietaryRestrictions(RecipeGeneratorDbContext context)
        {
            var tableName = "RecipeDietaryRestrictions";
            
            Console.WriteLine($"Seeding {tableName}...");
            
#pragma warning disable EF1002 // Risk of SQL injection - tableName is controlled internally
            var hasData = await context.Database
                .SqlQueryRaw<int>($"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [{tableName}]) THEN 1 ELSE 0 END AS int) AS Value")
                .FirstOrDefaultAsync();
#pragma warning restore EF1002

            if (hasData == 0)
            {
                var recipeDietary = await ReadFile<RecipeDietaryRestrictionDto>("test/RecipeDietaryRestrictions.json");
                Console.WriteLine($"  Inserting {recipeDietary.Count} recipe-dietary relationships");

                foreach (var rd in recipeDietary)
                {
                    await context.Database.ExecuteSqlAsync(
                        $"INSERT INTO [RecipeDietaryRestrictions] (RecipeId, DietaryRestrictionsId) VALUES ({rd.RecipeId}, {rd.DietaryRestrictionsId})");
                }
                Console.WriteLine($"✅ {tableName} seeded");
            }
        }

        private static async Task<List<T>> ReadFile<T>(string path)
        {
            Console.WriteLine($"  Reading file: {path}");
            
            if (!File.Exists(path))
            {
                Console.WriteLine($"  ⚠️ File not found: {path}");
                Console.WriteLine($"  Current directory: {Directory.GetCurrentDirectory()}");
                throw new FileNotFoundException($"Test data file not found: {path}");
            }
            
            var data = await File.ReadAllTextAsync(path);
            Console.WriteLine($"  File content length: {data.Length} characters");
            Console.WriteLine($"  First 100 chars: {(data.Length > 100 ? data.Substring(0, 100) : data)}");
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            
            var list = JsonSerializer.Deserialize<List<T>>(data, options);
            Console.WriteLine($"  Deserialized {list?.Count ?? 0} items");
            
            return list ?? new List<T>();
        }

        // DTOs for junction tables
        public class UserRecipeDto
        {
            public int UserId { get; set; }
            public int RecipeId { get; set; }
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
}