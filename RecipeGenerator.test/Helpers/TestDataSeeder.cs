using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.Models;

namespace RecipeGenerator.Test.Helpers
{
    public static class TestDataSeeder
    {
        // Whitelist of allowed tables for IDENTITY_INSERT operations
        private static readonly HashSet<string> AllowedTables = new()
        {
            "Users",
            "Recipes",
            "Ingredients",
            "Instructions",
            "DietaryRestrictions"
        };

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
                    // Check if we're using SQL Server
                    var isSqlServer = context.Database.IsSqlServer();
                    
                    if (isSqlServer)
                    {
                        // Validate table name to prevent SQL injection
                        var validTableName = ValidateTableName(tableName);
                        
                        Console.WriteLine($"  Setting IDENTITY_INSERT ON for {validTableName}");
                        // Note: IDENTITY_INSERT requires literal table names, cannot be parameterized
                        // Security is ensured through whitelist validation
                        await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{validTableName}] ON");
                    }

                    await seedAction();
                    
                    Console.WriteLine($"  Saving changes for {tableName}");
                    await context.SaveChangesAsync();
                    
                    if (isSqlServer)
                    {
                        var validTableName = ValidateTableName(tableName);
                        
                        Console.WriteLine($"  Setting IDENTITY_INSERT OFF for {validTableName}");
                        await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{validTableName}] OFF");
                    }

                    await transaction.CommitAsync();
                    
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

        /// <summary>
        /// Validates table name against whitelist to prevent SQL injection
        /// </summary>
        /// <param name="tableName">The table name to validate</param>
        /// <returns>The validated table name</returns>
        /// <exception cref="ArgumentException">Thrown when table name is not in the whitelist</exception>
        private static string ValidateTableName(string tableName)
        {
            if (!AllowedTables.Contains(tableName))
            {
                throw new ArgumentException(
                    $"Table '{tableName}' is not allowed for identity insert operations. " +
                    $"Allowed tables: {string.Join(", ", AllowedTables)}", 
                    nameof(tableName));
            }
            
            return tableName;
        }

        private static async Task SeedUserRecipes(RecipeGeneratorDbContext context)
        {
            var tableName = "UserRecipes";
            
            Console.WriteLine($"Seeding {tableName}...");
            
            // Use database-specific syntax
            var isSqlServer = context.Database.IsSqlServer();
            var checkQuery = isSqlServer
                ? $"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [{tableName}]) THEN 1 ELSE 0 END AS int) AS Value"
                : $"SELECT CASE WHEN EXISTS(SELECT 1 FROM \"{tableName}\") THEN 1 ELSE 0 END AS Value";

            var hasData = await context.Database
                .SqlQueryRaw<int>(checkQuery)
                .FirstOrDefaultAsync();

            if (hasData == 0)
            {
                var userRecipes = await ReadFile<UserRecipeDto>("test/UserSavedRecipes.json");
                Console.WriteLine($"  Inserting {userRecipes.Count} user-recipe relationships");

                // Use parameterized query - safe from SQL injection
                var insertQuery = isSqlServer
                    ? "INSERT INTO [UserRecipes] (UserId, RecipeId) VALUES ({0}, {1})"
                    : "INSERT INTO \"UserRecipes\" (UserId, RecipeId) VALUES ({0}, {1})";

                foreach (var ur in userRecipes)
                {
                    await context.Database.ExecuteSqlRawAsync(insertQuery, ur.UserId, ur.RecipeId);
                }
                Console.WriteLine($"✅ {tableName} seeded");
            }
        }

        private static async Task SeedRecipeIngredients(RecipeGeneratorDbContext context)
        {
            var tableName = "RecipeIngredients";
            
            Console.WriteLine($"Seeding {tableName}...");
            
            // Use database-specific syntax
            var isSqlServer = context.Database.IsSqlServer();
            var checkQuery = isSqlServer
                ? $"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [{tableName}]) THEN 1 ELSE 0 END AS int) AS Value"
                : $"SELECT CASE WHEN EXISTS(SELECT 1 FROM \"{tableName}\") THEN 1 ELSE 0 END AS Value";

            var hasData = await context.Database
                .SqlQueryRaw<int>(checkQuery)
                .FirstOrDefaultAsync();

            if (hasData == 0)
            {
                var recipeIngredients = await ReadFile<RecipeIngredientDto>("test/RecipeIngredients.json");
                Console.WriteLine($"  Inserting {recipeIngredients.Count} recipe-ingredient relationships");

                // Use parameterized query - safe from SQL injection
                var insertQuery = isSqlServer
                    ? "INSERT INTO [RecipeIngredients] (RecipeId, IngredientId) VALUES ({0}, {1})"
                    : "INSERT INTO \"RecipeIngredients\" (RecipeId, IngredientId) VALUES ({0}, {1})";

                foreach (var ri in recipeIngredients)
                {
                    await context.Database.ExecuteSqlRawAsync(insertQuery, ri.RecipeId, ri.IngredientId);
                }
                Console.WriteLine($"✅ {tableName} seeded");
            }
        }

        private static async Task SeedRecipeDietaryRestrictions(RecipeGeneratorDbContext context)
        {
            var tableName = "RecipeDietaryRestrictions";
            
            Console.WriteLine($"Seeding {tableName}...");
            
            // Use database-specific syntax
            var isSqlServer = context.Database.IsSqlServer();
            var checkQuery = isSqlServer
                ? $"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [{tableName}]) THEN 1 ELSE 0 END AS int) AS Value"
                : $"SELECT CASE WHEN EXISTS(SELECT 1 FROM \"{tableName}\") THEN 1 ELSE 0 END AS Value";

            var hasData = await context.Database
                .SqlQueryRaw<int>(checkQuery)
                .FirstOrDefaultAsync();

            if (hasData == 0)
            {
                var recipeDietary = await ReadFile<RecipeDietaryRestrictionDto>("test/RecipeDietaryRestrictions.json");
                Console.WriteLine($"  Inserting {recipeDietary.Count} recipe-dietary relationships");

                // Use parameterized query - safe from SQL injection
                var insertQuery = isSqlServer
                    ? "INSERT INTO [RecipeDietaryRestrictions] (RecipeId, DietaryRestrictionsId) VALUES ({0}, {1})"
                    : "INSERT INTO \"RecipeDietaryRestrictions\" (RecipeId, DietaryRestrictionsId) VALUES ({0}, {1})";

                foreach (var rd in recipeDietary)
                {
                    await context.Database.ExecuteSqlRawAsync(insertQuery, rd.RecipeId, rd.DietaryRestrictionsId);
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