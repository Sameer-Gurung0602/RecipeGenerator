using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.DTOs;
using RecipeGenerator.Models;
using RecipeGenerator.Services;
using Xunit;

namespace RecipeGenerator.Test.UnitTests.Services
{
    public class RecipeServiceDifficultySortingTests : IDisposable
    {
        private readonly RecipeGeneratorDbContext _context;
        private readonly RecipeService _recipeService;

        public RecipeServiceDifficultySortingTests()
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
                new DietaryRestrictions { DietaryRestrictionsId = 1, Name = "Vegan" }
            };
            _context.DietaryRestrictions.AddRange(dietaryRestrictions);

            // Seed Ingredients
            var ingredients = new List<Ingredient>
            {
                new Ingredient { IngredientId = 1, IngredientName = "Tomato" },
                new Ingredient { IngredientId = 2, IngredientName = "Cheese" }
            };
            _context.Ingredients.AddRange(ingredients);

            // Seed Instructions
            var instructions = new List<Instructions>
            {
                new Instructions { InstructionsId = 1, Instruction = "Cook easy recipe", RecipeId = 1 },
                new Instructions { InstructionsId = 2, Instruction = "Cook medium recipe", RecipeId = 2 },
                new Instructions { InstructionsId = 3, Instruction = "Cook hard recipe", RecipeId = 3 },
                new Instructions { InstructionsId = 4, Instruction = "Cook another easy", RecipeId = 4 },
                new Instructions { InstructionsId = 5, Instruction = "Cook another hard", RecipeId = 5 }
            };

            // Seed Recipes with DIFFERENT DIFFICULTIES
            var recipes = new List<Recipe>
            {
                // Recipe 1: Easy
                new Recipe
                {
                    RecipeId = 1,
                    Name = "Easy Tomato Salad",
                    Description = "Simple salad",
                    CookTime = 10,
                    Difficulty = "Easy",
                    CreatedAt = DateTime.Parse("2025-01-01"),
                    Img = "https://test.com/1.jpg",
                    InstructionsId = 1,
                    Ingredients = new List<Ingredient> { ingredients[0] },
                    DietaryRestrictions = new List<DietaryRestrictions> { dietaryRestrictions[0] },
                    Instructions = instructions[0]
                },
                // Recipe 2: Medium
                new Recipe
                {
                    RecipeId = 2,
                    Name = "Medium Pasta",
                    Description = "Moderate difficulty",
                    CookTime = 20,
                    Difficulty = "Medium",
                    CreatedAt = DateTime.Parse("2025-01-02"),
                    Img = "https://test.com/2.jpg",
                    InstructionsId = 2,
                    Ingredients = new List<Ingredient> { ingredients[0], ingredients[1] },
                    DietaryRestrictions = new List<DietaryRestrictions>(),
                    Instructions = instructions[1]
                },
                // Recipe 3: Hard
                new Recipe
                {
                    RecipeId = 3,
                    Name = "Hard Soufflé",
                    Description = "Complex dish",
                    CookTime = 60,
                    Difficulty = "Hard",
                    CreatedAt = DateTime.Parse("2025-01-03"),
                    Img = "https://test.com/3.jpg",
                    InstructionsId = 3,
                    Ingredients = new List<Ingredient> { ingredients[1] },
                    DietaryRestrictions = new List<DietaryRestrictions>(),
                    Instructions = instructions[2]
                },
                // Recipe 4: Easy (to test multiple same difficulty)
                new Recipe
                {
                    RecipeId = 4,
                    Name = "Easy Sandwich",
                    Description = "Quick meal",
                    CookTime = 5,
                    Difficulty = "Easy",
                    CreatedAt = DateTime.Parse("2025-01-04"),
                    Img = "https://test.com/4.jpg",
                    InstructionsId = 4,
                    Ingredients = new List<Ingredient> { ingredients[0] },
                    DietaryRestrictions = new List<DietaryRestrictions>(),
                    Instructions = instructions[3]
                },
                // Recipe 5: Hard (to test multiple same difficulty)
                new Recipe
                {
                    RecipeId = 5,
                    Name = "Hard Wellington",
                    Description = "Advanced cooking",
                    CookTime = 90,
                    Difficulty = "Hard",
                    CreatedAt = DateTime.Parse("2025-01-05"),
                    Img = "https://test.com/5.jpg",
                    InstructionsId = 5,
                    Ingredients = new List<Ingredient> { ingredients[1] },
                    DietaryRestrictions = new List<DietaryRestrictions>(),
                    Instructions = instructions[4]
                }
            };

            _context.Recipes.AddRange(recipes);
            _context.Instructions.AddRange(instructions);
            _context.SaveChanges();
        }

        #region GetAllRecipes Difficulty Sorting Tests

        [Fact]
        public async Task GetAllRecipes_SortsByDifficultyAscending_ReturnsEasyMediumHard()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(
                userId: 1,
                sortBy: "difficulty",
                sortOrder: "asc"
            );

            // Assert
            var recipes = result.ToList();
            recipes.Should().HaveCount(5);

            // First two should be Easy
            recipes[0].Difficulty.Should().Be("Easy");
            recipes[1].Difficulty.Should().Be("Easy");

            // Third should be Medium
            recipes[2].Difficulty.Should().Be("Medium");

            // Last two should be Hard
            recipes[3].Difficulty.Should().Be("Hard");
            recipes[4].Difficulty.Should().Be("Hard");
        }

        [Fact]
        public async Task GetAllRecipes_SortsByDifficultyDescending_ReturnsHardMediumEasy()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(
                userId: 1,
                sortBy: "difficulty",
                sortOrder: "desc"
            );

            // Assert
            var recipes = result.ToList();
            recipes.Should().HaveCount(5);

            // First two should be Hard
            recipes[0].Difficulty.Should().Be("Hard");
            recipes[1].Difficulty.Should().Be("Hard");

            // Third should be Medium
            recipes[2].Difficulty.Should().Be("Medium");

            // Last two should be Easy
            recipes[3].Difficulty.Should().Be("Easy");
            recipes[4].Difficulty.Should().Be("Easy");
        }

        [Fact]
        public async Task GetAllRecipes_DifficultyAscending_VerifyExactOrder()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(
                userId: 1,
                sortBy: "difficulty",
                sortOrder: "asc"
            );

            // Assert
            var difficulties = result.Select(r => r.Difficulty).ToList();
            
            // Should have exactly this order: Easy, Easy, Medium, Hard, Hard
            difficulties[0].Should().Be("Easy");
            difficulties[1].Should().Be("Easy");
            difficulties[2].Should().Be("Medium");
            difficulties[3].Should().Be("Hard");
            difficulties[4].Should().Be("Hard");
        }

        [Fact]
        public async Task GetAllRecipes_DifficultyDescending_VerifyExactOrder()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(
                userId: 1,
                sortBy: "difficulty",
                sortOrder: "desc"
            );

            // Assert
            var difficulties = result.Select(r => r.Difficulty).ToList();
            
            // Should have exactly this order: Hard, Hard, Medium, Easy, Easy
            difficulties[0].Should().Be("Hard");
            difficulties[1].Should().Be("Hard");
            difficulties[2].Should().Be("Medium");
            difficulties[3].Should().Be("Easy");
            difficulties[4].Should().Be("Easy");
        }

        #endregion

        #region GetMatchingRecipes Difficulty Sorting Tests

        [Fact]
        public async Task MatchRecipes_SortsByDifficultyAscending_ReturnsEasyMediumHard()
        {
            // Arrange - Use ingredient that all recipes have
            var ingredientIds = new List<int> { 1, 2 }; // Tomato and Cheese

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: ingredientIds,
                dietaryRestrictionIds: null,
                sortBy: "difficulty",
                sortOrder: "asc"
            );

            // Assert
            var recipes = result.ToList();
            recipes.Should().HaveCount(5, "all recipes have at least one matching ingredient");

            // Verify order: Easy → Easy → Medium → Hard → Hard
            recipes[0].Difficulty.Should().Be("Easy");
            recipes[1].Difficulty.Should().Be("Easy");
            recipes[2].Difficulty.Should().Be("Medium");
            recipes[3].Difficulty.Should().Be("Hard");
            recipes[4].Difficulty.Should().Be("Hard");
        }

        [Fact]
        public async Task MatchRecipes_SortsByDifficultyDescending_ReturnsHardMediumEasy()
        {
            // Arrange
            var ingredientIds = new List<int> { 1, 2 };

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: ingredientIds,
                dietaryRestrictionIds: null,
                sortBy: "difficulty",
                sortOrder: "desc"
            );

            // Assert
            var recipes = result.ToList();
            recipes.Should().HaveCount(5);

            // Verify order: Hard → Hard → Medium → Easy → Easy
            recipes[0].Difficulty.Should().Be("Hard");
            recipes[1].Difficulty.Should().Be("Hard");
            recipes[2].Difficulty.Should().Be("Medium");
            recipes[3].Difficulty.Should().Be("Easy");
            recipes[4].Difficulty.Should().Be("Easy");
        }

        [Fact]
        public async Task MatchRecipes_DifficultyAscending_VerifySpecificRecipes()
        {
            // Arrange
            var ingredientIds = new List<int> { 1 }; // Only Tomato

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: ingredientIds,
                dietaryRestrictionIds: null,
                sortBy: "difficulty",
                sortOrder: "asc"
            );

            // Assert
            var recipes = result.ToList();
            
            // Should have 3 recipes with Tomato (Recipes 1, 2, 4)
            recipes.Should().HaveCount(3);
            
            // Order should be: Easy (Recipe 1 or 4), Easy, Medium (Recipe 2)
            recipes[0].Difficulty.Should().Be("Easy");
            recipes[1].Difficulty.Should().Be("Easy");
            recipes[2].Difficulty.Should().Be("Medium");
        }

        [Fact]
        public async Task MatchRecipes_WithDietaryFilter_SortsByDifficulty()
        {
            // Arrange
            var ingredientIds = new List<int> { 1 };
            var dietaryRestrictionIds = new List<int> { 1 }; // Vegan

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: ingredientIds,
                dietaryRestrictionIds: dietaryRestrictionIds,
                sortBy: "difficulty",
                sortOrder: "asc"
            );

            // Assert
            var recipes = result.ToList();
            
            // Only Recipe 1 is vegan with tomato
            recipes.Should().ContainSingle();
            recipes[0].Difficulty.Should().Be("Easy");
            recipes[0].RecipeId.Should().Be(1);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetAllRecipes_DefaultSorting_DoesNotSortByDifficulty()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(userId: 1);

            // Assert
            var recipes = result.ToList();
            recipes.Should().HaveCount(5);
            
            // Should be sorted by RecipeId (default)
            recipes.Should().BeInAscendingOrder(r => r.RecipeId);
        }

        [Fact]
        public async Task GetAllRecipes_InvalidSortOrder_DefaultsToAscending()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(
                userId: 1,
                sortBy: "difficulty",
                sortOrder: "invalid"
            );

            // Assert
            var difficulties = result.Select(r => r.Difficulty).ToList();
            
            // Should default to ascending (Easy first)
            difficulties[0].Should().Be("Easy");
            difficulties[1].Should().Be("Easy");
        }

        [Fact]
        public async Task GetAllRecipes_CaseInsensitiveDifficulty_WorksCorrectly()
        {
            // This test verifies that the GetDifficultyOrder method handles case insensitivity
            // The recipes are seeded with "Easy", "Medium", "Hard" (proper case)
            
            // Act
            var result = await _recipeService.GetAllRecipes(
                userId: 1,
                sortBy: "DIFFICULTY", // Test uppercase
                sortOrder: "ASC"
            );

            // Assert
            var recipes = result.ToList();
            recipes[0].Difficulty.Should().Be("Easy");
            recipes[2].Difficulty.Should().Be("Medium");
            recipes[3].Difficulty.Should().Be("Hard");
        }

        #endregion

        #region Multiple Sorting Combinations

        [Fact]
        public async Task GetAllRecipes_SortsByCookTime_NotByDifficulty()
        {
            // Act
            var result = await _recipeService.GetAllRecipes(
                userId: 1,
                sortBy: "cooktime",
                sortOrder: "asc"
            );

            // Assert
            var recipes = result.ToList();
            recipes.Should().BeInAscendingOrder(r => r.CookTime);
            
            // Verify cook times: 5, 10, 20, 60, 90
            recipes[0].CookTime.Should().Be(5);
            recipes[1].CookTime.Should().Be(10);
            recipes[2].CookTime.Should().Be(20);
        }

        [Fact]
        public async Task MatchRecipes_DefaultSorting_SortsByMatchPercentage()
        {
            // Arrange
            var ingredientIds = new List<int> { 1 }; // Tomato

            // Act
            var result = await _recipeService.GetMatchingRecipes(
                userId: 1,
                ingredientIds: ingredientIds,
                dietaryRestrictionIds: null
                // No sortBy specified
            );

            // Assert
            var recipes = result.ToList();
            recipes.Should().BeInDescendingOrder(r => r.MatchPercentage);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }
    }
}