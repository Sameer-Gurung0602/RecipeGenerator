using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Models;

namespace RecipeGenerator.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(RecipeGeneratorDbContext context, IWebHostEnvironment env)
        {
            //applies existing migrations
            await context.Database.MigrateAsync();

            // choose environment for seed data (test/development)
            var environmentName = env.EnvironmentName.ToLower();
            var seedDataPath = Path.Combine(env.ContentRootPath, "SeedData", environmentName);

            // Seed Ingredients (no dependencies)
            if (!context.Ingredients.Any())
            {
                var ingredientsJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "Ingredients.json"));
                var ingredients = JsonSerializer.Deserialize<List<Ingredient>>(ingredientsJson);
                if (ingredients != null)
                {
                    context.Ingredients.AddRange(ingredients);
                    await context.SaveChangesAsync();
                }
            }

            // Seed DietaryRestrictions (no dependencies)
            if (!context.DietaryRestrictions.Any())
            {
                var dietaryJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "DietaryRestrictions.json"));
                var dietaryRestrictions = JsonSerializer.Deserialize<List<DietaryRestrictions>>(dietaryJson);
                if (dietaryRestrictions != null)
                {
                    context.DietaryRestrictions.AddRange(dietaryRestrictions);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Users (no dependencies)
            if (!context.Users.Any())
            {
                var usersJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "Users.json"));
                var users = JsonSerializer.Deserialize<List<User>>(usersJson);
                if (users != null)
                {
                    context.Users.AddRange(users);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Recipes (depends on Users)
            if (!context.Recipes.Any())
            {
                var recipesJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "Recipes.json"));
                var recipes = JsonSerializer.Deserialize<List<Recipe>>(recipesJson);
                if (recipes != null)
                {
                    context.Recipes.AddRange(recipes);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Instructions (depends on Recipes - one-to-one relationship)
            if (!context.Instructions.Any())
            {
                var instructionsJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "Instructions.json"));
                var instructions = JsonSerializer.Deserialize<List<Instructions>>(instructionsJson);
                if (instructions != null)
                {
                    context.Instructions.AddRange(instructions);
                    await context.SaveChangesAsync();
                }
            }

            // Seed RecipeIngredients (many-to-many: Recipe + Ingredient)
            if (!context.RecipeIngredients.Any())
            {
                var recipeIngredientsJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "RecipeIngredients.json"));
                var recipeIngredients = JsonSerializer.Deserialize<List<RecipeIngredients>>(recipeIngredientsJson);
                if (recipeIngredients != null)
                {
                    context.RecipeIngredients.AddRange(recipeIngredients);
                    await context.SaveChangesAsync();
                }
            }

            // Seed RecipeDietaryRestrictions (many-to-many: Recipe + DietaryRestrictions)
            if (!context.RecipeDietaryRestrictions.Any())
            {
                var recipeDietaryJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "RecipeDietaryRestrictions.json"));
                var recipeDietary = JsonSerializer.Deserialize<List<RecipeDietaryRestrictions>>(recipeDietaryJson);
                if (recipeDietary != null)
                {
                    context.RecipeDietaryRestrictions.AddRange(recipeDietary);
                    await context.SaveChangesAsync();
                }
            }

            // Seed UserSavedRecipes (many-to-many: User + Recipe)
            if (!context.UserSavedRecipes.Any())
            {
                var userSavedJson = await File.ReadAllTextAsync(
                    Path.Combine(seedDataPath, "UserSavedRecipes.json"));
                var userSavedRecipes = JsonSerializer.Deserialize<List<UserSavedRecipes>>(userSavedJson);
                if (userSavedRecipes != null)
                {
                    context.UserSavedRecipes.AddRange(userSavedRecipes);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
