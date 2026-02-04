using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using RecipeGenerator.Data;
using RecipeGenerator.DTOs;
using RecipeGenerator.Models;
using RecipeGenerator.Services;
using Xunit;

namespace RecipeGenerator.Test.UnitTests.Services
{
    public class RecipeServiceTests : IDisposable
    {
        private readonly RecipeGeneratorDbContext _context;
        private readonly RecipeService _recipeService;

        public RecipeServiceTests()
        {
            // Setup in-memory SQLite database
            var options = new DbContextOptionsBuilder<RecipeGeneratorDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new RecipeGeneratorDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            // Seed test data
            SeedTestData();

            _recipeService = new RecipeService(_context);
        }

        private void SeedTestData()
        {
            // Seed Users
            var users = new List<User>
            {
                new User { UserId = 1, Username = "TestUser" }
            };
            _context.Users.AddRange(users);

            // Seed Dietary Restrictions
            var dietaryRestrictions = new List<DietaryRestrictions>
            {
                new DietaryRestrictions { DietaryRestrictionsId = 1, Name = "Vegan" },
                new DietaryRestrictions { DietaryRestrictionsId = 2, Name = "Vegetarian" },
                new DietaryRestrictions { DietaryRestrictionsId = 3, Name = "Gluten-Free" },
                new DietaryRestrictions { DietaryRestrictionsId = 5, Name = "Keto" }
            };
            _context.DietaryRestrictions.AddRange(dietaryRestrictions);

            // Seed Ingredients
            var ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientId = 1, IngredientName = "Chicken Breast" },
                new Ingredient { IngredientId = 11, IngredientName = "Mushrooms" },
                new Ingredient { IngredientId = 10, IngredientName = "Zucchini" },
                new Ingredient { IngredientId = 23, IngredientName = "Beef" },
                new Ingredient { IngredientId = 20, IngredientName = "Chickpeas" },
                new Ingredient { IngredientId = 13, IngredientName = "Eggs" }
            };
            _context.Ingredients.AddRange(ingredients);

            // Seed Instructions
            var instructions = new List<Instructions>
            {
                new Instructions { InstructionsId = 1, Instruction = "Cook chicken", RecipeId = 1 },
                new Instructions { InstructionsId = 2, Instruction = "Stir fry veggies", RecipeId = 2 },
                new Instructions { InstructionsId = 3, Instruction = "Make curry", RecipeId = 3 },
                new Instructions { InstructionsId = 4, Instruction = "Cook beef skillet", RecipeId = 4 },
                new Instructions { InstructionsId = 5, Instruction = "Cook omelette", RecipeId = 5 }
            };

            // Seed Recipes
            var recipes = new List<Recipe>
            {
                // Recipe 1: Chicken with Mushrooms (NOT vegan)
                new Recipe
                {
                    RecipeId = 1,
                    Name = "Chicken Mushroom Stir Fry",
                    Description = "Chicken with mushrooms",
                    CookTime = 30,
                    Difficulty = "Easy",
                    CreatedAt = DateTime.Parse("2025-01-01"),
                    Img = "https://test.com/1.jpg",
                    InstructionsId = 1,
                    Ingredients = new List<Ingredient> { ingredients[0], ingredients[1] }, // Chicken, Mushrooms
                    DietaryRestrictions = new List<DietaryRestrictions> { dietaryRestrictions[2] }, // Gluten-Free
                    Instructions = instructions[0]
                },
                // Recipe 2: Vegetable Stir Fry (Vegan, has mushrooms)
                new Recipe
                {
                    RecipeId = 2,
                    Name = "Vegetable Stir Fry",
                    Description = "Mixed veggies",
                    CookTime = 20,
                    Difficulty = "Easy",
                    CreatedAt = DateTime.Parse("2025-01-02"),
                    Img = "https://test.com/2.jpg",
                    InstructionsId = 2,
                    Ingredients = new List<Ingredient> { ingredients[1], ingredients[2] }, // Mushrooms, Zucchini
                    DietaryRestrictions = new List<DietaryRestrictions> { dietaryRestrictions[0], dietaryRestrictions[1] }, // Vegan, Vegetarian
                    Instructions = instructions[1]
                },
                // Recipe 3: Chickpea Curry (Vegan, NO mushrooms)
                new Recipe
                {
                    RecipeId = 3,
                    Name = "Chickpea Curry",
                    Description = "Creamy curry",
                    CookTime = 30,
                    Difficulty = "Easy",
                    CreatedAt = DateTime.Parse("2025-01-03"),
                    Img = "https://test.com/3.jpg",
                    InstructionsId = 3,
                    Ingredients = new List<Ingredient> { ingredients[4] }, // Chickpeas
                    DietaryRestrictions = new List<DietaryRestrictions> { dietaryRestrictions[0], dietaryRestrictions[1] }, // Vegan, Vegetarian
                    Instructions = instructions[2]
                },
                // Recipe 4: Keto Beef Skillet (Keto, has beef and mushrooms)
                new Recipe
                {
                    RecipeId = 4,
                    Name = "Keto Beef Skillet",
                    Description = "Beef and veggies",
                    CookTime = 25,
                    Difficulty = "Medium",
                    CreatedAt = DateTime.Parse("2025-01-04"),
                    Img = "https://test.com/4.jpg",
                    InstructionsId = 4,
                    Ingredients = new List<Ingredient> { ingredients[3], ingredients[1], ingredients[2] }, // Beef, Mushrooms, Zucchini
                    DietaryRestrictions = new List<DietaryRestrictions> { dietaryRestrictions[3] }, // Keto
                    Instructions = instructions[3]
                },
                // Recipe 5: Veggie Omelette (Vegetarian, has eggs and mushrooms)
                new Recipe
                {
                    RecipeId = 5,
                    Name = "Veggie Omelette",
                    Description = "Eggs with veggies",
                    CookTime = 15,
                    Difficulty = "Easy",
                    CreatedAt = DateTime.Parse("2025-01-05"),
                    Img = "https://test.com/5.jpg",
                    InstructionsId = 5,
                    Ingredients = new List<Ingredient> { ingredients[5], ingredients[1] }, // Eggs, Mushrooms
                    DietaryRestrictions = new List<DietaryRestrictions> { dietaryRestrictions[1] }, // Vegetarian
                    Instructions = instructions[4]
                }
            };

            _context.Recipes.AddRange(recipes);
            _context.Instructions.AddRange(instructions);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetMatchingRecipes_WithVeganFilter_ExcludesChickenRecipes()
        {
            // Arrange
            var chickenId = 1; // Chicken Breast
            var mushroomsId = 11; // Mushrooms
            var veganId = 1; // Vegan dietary restriction

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { chickenId, mushroomsId },
                dietaryRestrictionIds: new List<int> { veganId }
            );

            // Assert
            result.Should().NotBeEmpty("there are vegan recipes with mushrooms");
            result.Should().NotContain(r => r.RecipeId == 1, "Recipe 1 has chicken and is NOT vegan");
            result.Should().Contain(r => r.RecipeId == 2, "Recipe 2 is vegan and has mushrooms");
            result.Should().NotContain(r => r.RecipeId == 3, "Recipe 3 is vegan but has NO mushrooms");
        }

        [Fact]
        public async Task GetMatchingRecipes_WithVeganAndChicken_ReturnsEmptyList()
        {
            // Arrange
            var chickenId = 1; // Chicken Breast
            var veganId = 1; // Vegan dietary restriction

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { chickenId },
                dietaryRestrictionIds: new List<int> { veganId }
            );

            // Assert
            result.Should().BeEmpty("no vegan recipes contain chicken");
        }

        [Fact]
        public async Task GetMatchingRecipes_WithMushroomsOnly_ReturnsAllRecipesWithMushrooms()
        {
            // Arrange
            var mushroomsId = 11; // Mushrooms

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { mushroomsId },
                dietaryRestrictionIds: null
            );

            // Assert
            result.Should().HaveCount(4, "4 recipes contain mushrooms");
            result.Should().Contain(r => r.RecipeId == 1); // Chicken Mushroom
            result.Should().Contain(r => r.RecipeId == 2); // Vegan Stir Fry
            result.Should().Contain(r => r.RecipeId == 4); // Keto Beef
            result.Should().Contain(r => r.RecipeId == 5); // Veggie Omelette
            result.Should().NotContain(r => r.RecipeId == 3, "Recipe 3 has no mushrooms");
        }

        [Fact]
        public async Task GetMatchingRecipes_WithVeganFilterAndMushrooms_ReturnsOnlyVeganMushroomRecipes()
        {
            // Arrange
            var mushroomsId = 11;
            var veganId = 1;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { mushroomsId },
                dietaryRestrictionIds: new List<int> { veganId }
            );

            // Assert
            result.Should().ContainSingle("only Recipe 2 is vegan AND has mushrooms");
            result.First().RecipeId.Should().Be(2);
            result.First().Name.Should().Be("Vegetable Stir Fry");
            result.First().DietaryRestrictions.Should().Contain("Vegan");
        }

        [Fact]
        public async Task GetMatchingRecipes_WithKetoFilterAndBeef_ReturnsOnlyKetoBeefRecipes()
        {
            // Arrange
            var beefId = 23;
            var ketoId = 5;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { beefId },
                dietaryRestrictionIds: new List<int> { ketoId }
            );

            // Assert
            result.Should().ContainSingle("only Recipe 4 is keto AND has beef");
            result.First().RecipeId.Should().Be(4);
            result.First().Name.Should().Be("Keto Beef Skillet");
        }

        [Fact]
        public async Task GetMatchingRecipes_CalculatesCorrectMatchPercentage()
        {
            // Arrange - Recipe 2 has Mushrooms and Zucchini (2 ingredients)
            var mushroomsId = 11;
            var zucchiniId = 10;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { mushroomsId, zucchiniId },
                dietaryRestrictionIds: null
            );

            // Assert
            var recipe2 = result.First(r => r.RecipeId == 2);
            recipe2.MatchPercentage.Should().Be(100, "user has all ingredients");
            recipe2.IngredientsMatched.Should().Be(2);
            recipe2.TotalIngredientsRequired.Should().Be(2);
            recipe2.MissingIngredients.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMatchingRecipes_ShowsPartialMatch()
        {
            // Arrange - Recipe 4 has Beef, Mushrooms, Zucchini (3 ingredients)
            // User only has Mushrooms
            var mushroomsId = 11;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { mushroomsId },
                dietaryRestrictionIds: null
            );

            // Assert
            var recipe4 = result.First(r => r.RecipeId == 4);
            recipe4.MatchPercentage.Should().Be(33, "user has 1 of 3 ingredients (33%)");
            recipe4.IngredientsMatched.Should().Be(1);
            recipe4.TotalIngredientsRequired.Should().Be(3);
            recipe4.MissingIngredients.Should().HaveCount(2);
            recipe4.MissingIngredients.Should().Contain("Beef");
            recipe4.MissingIngredients.Should().Contain("Zucchini");
        }

        [Fact]
        public async Task GetMatchingRecipes_WithVegetarianFilter_ExcludesBeefAndChicken()
        {
            // Arrange
            var mushroomsId = 11;
            var vegetarianId = 2;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { mushroomsId },
                dietaryRestrictionIds: new List<int> { vegetarianId }
            );

            // Assert
            result.Should().HaveCount(2, "Recipes 2 and 5 are vegetarian with mushrooms");
            result.Should().Contain(r => r.RecipeId == 2); // Vegan Stir Fry
            result.Should().Contain(r => r.RecipeId == 5); // Veggie Omelette
            result.Should().NotContain(r => r.RecipeId == 1, "has chicken");
            result.Should().NotContain(r => r.RecipeId == 4, "has beef");
        }

        [Fact]
        public async Task GetMatchingRecipes_SortsByMatchPercentageDescendingByDefault()
        {
            // Arrange
            var mushroomsId = 11;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { mushroomsId },
                dietaryRestrictionIds: null
            );

            // Assert
            result.Should().BeInDescendingOrder(r => r.MatchPercentage);
        }

        [Fact]
        public async Task GetMatchingRecipes_SortsByCookTimeAscending()
        {
            // Arrange
            var mushroomsId = 11;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { mushroomsId },
                dietaryRestrictionIds: null,
                sortBy: "cooktime",
                sortOrder: "asc"
            );

            // Assert
            result.Should().BeInAscendingOrder(r => r.CookTime);
        }

        [Fact]
        public async Task GetMatchingRecipes_WithNoIngredients_ReturnsAllRecipesWithDietaryFilter()
        {
            // Arrange
            var veganId = 1;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: null,
                dietaryRestrictionIds: new List<int> { veganId }
            );

            // Assert
            result.Should().HaveCount(2, "2 recipes are vegan");
            result.Should().Contain(r => r.RecipeId == 2);
            result.Should().Contain(r => r.RecipeId == 3);
            result.Should().AllSatisfy(r =>
            {
                r.MatchPercentage.Should().Be(0, "no ingredients provided");
                r.IngredientsMatched.Should().Be(0);
                r.MissingIngredients.Should().NotBeEmpty();
            });
        }

        [Fact]
        public async Task GetMatchingRecipes_FiltersOutRecipesWithoutSearchedIngredients()
        {
            // Arrange - Search for chicken (only Recipe 1 has it)
            var chickenId = 1;

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: new List<int> { chickenId },
                dietaryRestrictionIds: null
            );

            // Assert
            result.Should().ContainSingle("only Recipe 1 has chicken");
            result.First().RecipeId.Should().Be(1);
        }

        [Fact]
        public async Task GetAllRecipes_ReturnsAllRecipes()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(userId: 1);

            // Assert
            result.Should().HaveCount(5, "5 recipes were seeded");
        }

        [Fact]
        public async Task GetRecipeById_ReturnsCorrectRecipe()
        {
            // Act
            var result = await _recipeService.GetRecipeById(id: 2, userId: 1);

            // Assert
            result.Should().NotBeNull();
            result!.RecipeId.Should().Be(2);
            result.Name.Should().Be("Vegetable Stir Fry");
            result.Ingredients.Should().Contain("Mushrooms");
            result.DietaryRestrictions.Should().Contain("Vegan");
        }

        [Fact]
        public async Task GetRecipeById_ReturnsNull_WhenRecipeDoesNotExist()
        {
            // Act
            var result = await _recipeService.GetRecipeById(id: 999, userId: 1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllIngredients_ReturnsAllIngredients()
        {
            // Act
            var result = await _recipeService.GetAllIngredients();

            // Assert
            result.Should().HaveCount(6, "6 ingredients were seeded");
            result.Should().Contain(i => i.IngredientName == "Chicken Breast");
            result.Should().Contain(i => i.IngredientName == "Mushrooms");
        }

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }
    }
}