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

            // Seed Ingredients (no dependencies)
            if (!context.Ingredients.Any())
            {
                var ingredientsJson = await File.ReadAllTextAsync(
                    Path.Combine(env.ContentRootPath, "SeedData", "Ingredients.json"));
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
                    Path.Combine(env.ContentRootPath, "SeedData", "DietaryRestrictions.json"));
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
                    Path.Combine(env.ContentRootPath, "SeedData", "Users.json"));
                var users = JsonSerializer.Deserialize<List<User>>(usersJson);
                if (users != null)
                {
                    context.Users.AddRange(users);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Instructions (no dependencies)
            if (!context.Instructions.Any())
            {
                var instructions = new List<Instructions>
                {
                    new Instructions { Instruction = "Step 1: Season the chicken breasts with salt and pepper.\nStep 2: Heat olive oil in a pan over medium heat.\nStep 3: Add minced garlic and sauté until fragrant.\nStep 4: Add chicken to the pan and cook for 6–7 minutes per side until golden brown.\nStep 5: Remove chicken and serve hot." },
                    new Instructions { Instruction = "Step 1: Bring a pot of salted water to a boil and cook pasta according to package instructions.\nStep 2: Heat olive oil in a saucepan and sauté garlic.\nStep 3: Add crushed tomatoes and simmer for 10 minutes.\nStep 4: Season with salt and basil.\nStep 5: Toss pasta with sauce and serve." },
                    new Instructions { Instruction = "Step 1: Chop all vegetables into bite-sized pieces.\nStep 2: Heat oil in a wok over high heat.\nStep 3: Add garlic and stir for 30 seconds.\nStep 4: Add vegetables and stir-fry for 5–7 minutes.\nStep 5: Season and serve immediately." },
                    new Instructions { Instruction = "Step 1: Preheat oven to 375°F (190°C).\nStep 2: Place salmon on a baking tray.\nStep 3: Drizzle with olive oil and lemon juice.\nStep 4: Season with salt and pepper.\nStep 5: Bake for 25–30 minutes and serve." },
                    new Instructions { Instruction = "Step 1: Cook rice according to package instructions.\nStep 2: Heat oil in a pan and brown the beef.\nStep 3: Season beef with salt and pepper.\nStep 4: Assemble bowl with rice and beef.\nStep 5: Serve warm." },
                    new Instructions { Instruction = "Step 1: Heat oil in a pot over medium heat.\nStep 2: Add curry powder and toast for 30 seconds.\nStep 3: Add chickpeas and coconut milk.\nStep 4: Simmer for 15 minutes.\nStep 5: Serve with rice or bread." },
                    new Instructions { Instruction = "Step 1: Whisk eggs in a bowl.\nStep 2: Chop vegetables.\nStep 3: Heat butter in a pan.\nStep 4: Add vegetables and sauté for 2 minutes.\nStep 5: Pour eggs over veggies and cook until set." },
                    new Instructions { Instruction = "Step 1: Rinse quinoa under cold water.\nStep 2: Cook quinoa according to package directions.\nStep 3: Chop tomatoes and lettuce.\nStep 4: Mix quinoa with vegetables.\nStep 5: Drizzle with olive oil and serve." },
                    new Instructions { Instruction = "Step 1: Toast bread slices until golden.\nStep 2: Mash avocado in a bowl.\nStep 3: Season avocado with salt and lemon juice.\nStep 4: Spread avocado on toast.\nStep 5: Serve immediately." },
                    new Instructions { Instruction = "Step 1: Preheat oven to 375°F (190°C).\nStep 2: Cook rice until tender.\nStep 3: Cut tops off bell peppers and remove seeds.\nStep 4: Stuff peppers with rice.\nStep 5: Bake for 40 minutes." },
                    new Instructions { Instruction = "Step 1: Spiralize zucchini into noodles.\nStep 2: Heat olive oil in a pan.\nStep 3: Add garlic and sauté.\nStep 4: Add zucchini noodles and cook for 2–3 minutes.\nStep 5: Serve immediately." },
                    new Instructions { Instruction = "Step 1: Heat oil in a skillet.\nStep 2: Add beef and cook until browned.\nStep 3: Add chopped vegetables.\nStep 4: Season with salt and pepper.\nStep 5: Cook until vegetables are tender." },
                    new Instructions { Instruction = "Step 1: Preheat oven to 400°F (200°C).\nStep 2: Peel and cube sweet potatoes.\nStep 3: Toss with olive oil and salt.\nStep 4: Roast for 25 minutes.\nStep 5: Serve in a bowl." },
                    new Instructions { Instruction = "Step 1: Heat butter in a pan.\nStep 2: Add mushrooms and sauté.\nStep 3: Add rice and stir.\nStep 4: Slowly add broth while stirring.\nStep 5: Cook until creamy." },
                    new Instructions { Instruction = "Step 1: Spoon yogurt into a bowl.\nStep 2: Add sliced fruit on top.\nStep 3: Drizzle with honey if desired.\nStep 4: Add granola.\nStep 5: Serve chilled." },
                    new Instructions { Instruction = "Step 1: Cook and dice chicken.\nStep 2: Chop lettuce.\nStep 3: Add chicken to salad bowl.\nStep 4: Toss with dressing.\nStep 5: Serve cold." },
                    new Instructions { Instruction = "Step 1: Heat black beans in a pan.\nStep 2: Warm taco shells.\nStep 3: Fill shells with beans.\nStep 4: Add toppings.\nStep 5: Serve immediately." },
                    new Instructions { Instruction = "Step 1: Season steak with salt and pepper.\nStep 2: Heat grill or pan.\nStep 3: Cook steak to desired doneness.\nStep 4: Roast vegetables.\nStep 5: Serve together." },
                    new Instructions { Instruction = "Step 1: Crack eggs into a bowl.\nStep 2: Whisk with salt and pepper.\nStep 3: Heat butter in pan.\nStep 4: Add spinach and cook briefly.\nStep 5: Add eggs and scramble." },
                    new Instructions { Instruction = "Step 1: Rinse rice.\nStep 2: Add rice and coconut milk to pot.\nStep 3: Bring to boil.\nStep 4: Reduce heat and simmer 15 minutes.\nStep 5: Fluff and serve." },
                    new Instructions { Instruction = "Step 1: Chop tomatoes.\nStep 2: Sauté garlic in pot.\nStep 3: Add tomatoes and simmer.\nStep 4: Add basil.\nStep 5: Blend until smooth." },
                    new Instructions { Instruction = "Step 1: Season salmon with salt and pepper.\nStep 2: Grill salmon for 4–5 minutes per side.\nStep 3: Cook quinoa.\nStep 4: Assemble bowl with quinoa and salmon.\nStep 5: Serve warm." }
                };
                
                context.Instructions.AddRange(instructions);
                await context.SaveChangesAsync();
            }

            // Seed Recipes (depends on Instructions)
            if (!context.Recipes.Any())
            {
                var instructions = context.Instructions.OrderBy(i => i.Id).ToList();
                
                if (instructions.Count >= 22)
                {
                    var recipes = new List<Recipe>
                    {
                        new Recipe { Name = "Garlic Chicken", Description = "Pan-seared chicken with garlic sauce", CookTime = 30, InstructionsId = instructions[0].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 1) },
                        new Recipe { Name = "Spaghetti Marinara", Description = "Classic tomato pasta", CookTime = 25, InstructionsId = instructions[1].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 2) },
                        new Recipe { Name = "Vegetable Stir Fry", Description = "Mixed veggies in garlic sauce", CookTime = 20, InstructionsId = instructions[2].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 3) },
                        new Recipe { Name = "Salmon Lemon Bake", Description = "Baked salmon with lemon", CookTime = 35, InstructionsId = instructions[3].Id, Difficulty = "Medium", CreatedAt = new DateTime(2025, 1, 4) },
                        new Recipe { Name = "Beef Rice Bowl", Description = "Beef over seasoned rice", CookTime = 40, InstructionsId = instructions[4].Id, Difficulty = "Medium", CreatedAt = new DateTime(2025, 1, 5) },
                        new Recipe { Name = "Chickpea Curry", Description = "Creamy coconut chickpea curry", CookTime = 30, InstructionsId = instructions[5].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 6) },
                        new Recipe { Name = "Veggie Omelette", Description = "Egg omelette with veggies", CookTime = 15, InstructionsId = instructions[6].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 7) },
                        new Recipe { Name = "Quinoa Salad", Description = "Fresh quinoa veggie salad", CookTime = 20, InstructionsId = instructions[7].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 8) },
                        new Recipe { Name = "Avocado Toast", Description = "Toasted bread with avocado", CookTime = 10, InstructionsId = instructions[8].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 9) },
                        new Recipe { Name = "Stuffed Peppers", Description = "Bell peppers stuffed with rice", CookTime = 45, InstructionsId = instructions[9].Id, Difficulty = "Medium", CreatedAt = new DateTime(2025, 1, 10) },
                        new Recipe { Name = "Zucchini Noodles", Description = "Low-carb zucchini pasta", CookTime = 15, InstructionsId = instructions[10].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 11) },
                        new Recipe { Name = "Keto Beef Skillet", Description = "Beef and veggie keto skillet", CookTime = 25, InstructionsId = instructions[11].Id, Difficulty = "Medium", CreatedAt = new DateTime(2025, 1, 12) },
                        new Recipe { Name = "Sweet Potato Bowl", Description = "Roasted sweet potato bowl", CookTime = 30, InstructionsId = instructions[12].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 13) },
                        new Recipe { Name = "Mushroom Risotto", Description = "Creamy mushroom rice", CookTime = 40, InstructionsId = instructions[13].Id, Difficulty = "Hard", CreatedAt = new DateTime(2025, 1, 14) },
                        new Recipe { Name = "Greek Yogurt Parfait", Description = "Yogurt with fruit", CookTime = 5, InstructionsId = instructions[14].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 15) },
                        new Recipe { Name = "Chicken Salad", Description = "Fresh chicken salad", CookTime = 20, InstructionsId = instructions[15].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 16) },
                        new Recipe { Name = "Black Bean Tacos", Description = "Veggie black bean tacos", CookTime = 25, InstructionsId = instructions[16].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 17) },
                        new Recipe { Name = "Paleo Steak Plate", Description = "Steak with veggies", CookTime = 35, InstructionsId = instructions[17].Id, Difficulty = "Medium", CreatedAt = new DateTime(2025, 1, 18) },
                        new Recipe { Name = "Spinach Scramble", Description = "Eggs scrambled with spinach", CookTime = 10, InstructionsId = instructions[18].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 19) },
                        new Recipe { Name = "Coconut Rice", Description = "Creamy coconut rice", CookTime = 25, InstructionsId = instructions[19].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 20) },
                        new Recipe { Name = "Tomato Basil Soup", Description = "Creamy tomato basil soup", CookTime = 30, InstructionsId = instructions[20].Id, Difficulty = "Easy", CreatedAt = new DateTime(2025, 1, 21) },
                        new Recipe { Name = "Grilled Salmon Bowl", Description = "Salmon with quinoa", CookTime = 30, InstructionsId = instructions[21].Id, Difficulty = "Medium", CreatedAt = new DateTime(2025, 1, 22) }
                    };
                    
                    context.Recipes.AddRange(recipes);
                    await context.SaveChangesAsync();
                }
            }

            // Seed RecipeIngredients (many-to-many: Recipe + Ingredient)
            if (!context.RecipeIngredients.Any())
            {
                var recipes = context.Recipes.OrderBy(r => r.RecipeId).ToList();
                var ingredients = context.Ingredients.OrderBy(i => i.IngredientId).ToList();
                
                if (recipes.Count >= 22 && ingredients.Count >= 30)
                {
                    var recipeIngredients = new List<RecipeIngredients>
                    {
                        new RecipeIngredients { RecipeId = recipes[0].RecipeId, IngredientId = ingredients[0].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[0].RecipeId, IngredientId = ingredients[1].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[0].RecipeId, IngredientId = ingredients[2].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[1].RecipeId, IngredientId = ingredients[3].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[1].RecipeId, IngredientId = ingredients[6].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[1].RecipeId, IngredientId = ingredients[5].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[2].RecipeId, IngredientId = ingredients[8].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[2].RecipeId, IngredientId = ingredients[9].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[2].RecipeId, IngredientId = ingredients[10].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[3].RecipeId, IngredientId = ingredients[17].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[3].RecipeId, IngredientId = ingredients[16].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[4].RecipeId, IngredientId = ingredients[22].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[4].RecipeId, IngredientId = ingredients[11].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[5].RecipeId, IngredientId = ingredients[19].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[5].RecipeId, IngredientId = ingredients[20].IngredientId },
                        new RecipeIngredients { RecipeId = recipes[5].RecipeId, IngredientId = ingredients[21].IngredientId },
                    };
                    
                    context.RecipeIngredients.AddRange(recipeIngredients);
                    await context.SaveChangesAsync();
                }
            }

            // Seed RecipeDietaryRestrictions (many-to-many: Recipe + DietaryRestrictions)
            if (!context.RecipeDietaryRestrictions.Any())
            {
                var recipes = context.Recipes.OrderBy(r => r.RecipeId).ToList();
                var dietary = context.DietaryRestrictions.OrderBy(d => d.Id).ToList();
                
                if (recipes.Count >= 18 && dietary.Count >= 6)
                {
                    var recipeDietary = new List<RecipeDietaryRestrictions>
                    {
                        new RecipeDietaryRestrictions { RecipeID = recipes[2].RecipeId, DietaryRestrictionID = dietary[1].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[5].RecipeId, DietaryRestrictionID = dietary[0].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[5].RecipeId, DietaryRestrictionID = dietary[1].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[7].RecipeId, DietaryRestrictionID = dietary[0].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[7].RecipeId, DietaryRestrictionID = dietary[1].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[10].RecipeId, DietaryRestrictionID = dietary[2].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[10].RecipeId, DietaryRestrictionID = dietary[4].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[11].RecipeId, DietaryRestrictionID = dietary[4].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[16].RecipeId, DietaryRestrictionID = dietary[0].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[16].RecipeId, DietaryRestrictionID = dietary[1].Id },
                        new RecipeDietaryRestrictions { RecipeID = recipes[17].RecipeId, DietaryRestrictionID = dietary[5].Id },
                    };
                    
                    context.RecipeDietaryRestrictions.AddRange(recipeDietary);
                    await context.SaveChangesAsync();
                }
            }

            // Seed UserSavedRecipes (many-to-many: User + Recipe)
            if (!context.UserSavedRecipes.Any())
            {
                var users = context.Users.OrderBy(u => u.UserId).ToList();
                var recipes = context.Recipes.OrderBy(r => r.RecipeId).ToList();
                
                if (users.Count >= 8 && recipes.Count >= 22)
                {
                    var userSavedRecipes = new List<UserSavedRecipes>
                    {
                        new UserSavedRecipes { UserId = users[0].UserId, RecipeId = recipes[1].RecipeId, SavedAt = new DateTime(2025, 1, 10) },
                        new UserSavedRecipes { UserId = users[0].UserId, RecipeId = recipes[3].RecipeId, SavedAt = new DateTime(2025, 1, 12) },
                        new UserSavedRecipes { UserId = users[1].UserId, RecipeId = recipes[5].RecipeId, SavedAt = new DateTime(2025, 1, 13) },
                        new UserSavedRecipes { UserId = users[1].UserId, RecipeId = recipes[7].RecipeId, SavedAt = new DateTime(2025, 1, 14) },
                        new UserSavedRecipes { UserId = users[2].UserId, RecipeId = recipes[9].RecipeId, SavedAt = new DateTime(2025, 1, 15) },
                        new UserSavedRecipes { UserId = users[2].UserId, RecipeId = recipes[13].RecipeId, SavedAt = new DateTime(2025, 1, 16) },
                        new UserSavedRecipes { UserId = users[3].UserId, RecipeId = recipes[5].RecipeId, SavedAt = new DateTime(2025, 1, 17) },
                        new UserSavedRecipes { UserId = users[3].UserId, RecipeId = recipes[16].RecipeId, SavedAt = new DateTime(2025, 1, 18) },
                        new UserSavedRecipes { UserId = users[4].UserId, RecipeId = recipes[10].RecipeId, SavedAt = new DateTime(2025, 1, 19) },
                        new UserSavedRecipes { UserId = users[4].UserId, RecipeId = recipes[11].RecipeId, SavedAt = new DateTime(2025, 1, 20) },
                        new UserSavedRecipes { UserId = users[5].UserId, RecipeId = recipes[6].RecipeId, SavedAt = new DateTime(2025, 1, 21) },
                        new UserSavedRecipes { UserId = users[5].UserId, RecipeId = recipes[18].RecipeId, SavedAt = new DateTime(2025, 1, 22) },
                        new UserSavedRecipes { UserId = users[6].UserId, RecipeId = recipes[21].RecipeId, SavedAt = new DateTime(2025, 1, 23) },
                        new UserSavedRecipes { UserId = users[7].UserId, RecipeId = recipes[17].RecipeId, SavedAt = new DateTime(2025, 1, 24) },
                    };
                    
                    context.UserSavedRecipes.AddRange(userSavedRecipes);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
