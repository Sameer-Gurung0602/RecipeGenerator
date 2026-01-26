using RecipeGenerator.Models;
using Xunit;

namespace RecipeGenerator.test.IntegrationTests
{
    public class ModelTests
    {
        [Fact]
        public void User_CanBeCreated_WithValidProperties()
        {
            // Arrange & Act
            var user = new User
            {
                UserId = 1,
                Username = "TestUser"
            };

            // Assert
            Assert.Equal(1, user.UserId);
            Assert.Equal("TestUser", user.Username);
        }

        [Fact]
        public void Recipe_CanBeCreated_WithValidProperties()
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                RecipeId = 1,
                Name = "Test Recipe",
                Description = "A test recipe",
                CookTime = 30,
                Difficulty = "Easy",
                CreatedAt = DateTime.Now
            };

            // Assert
            Assert.Equal(1, recipe.RecipeId);
            Assert.Equal("Test Recipe", recipe.Name);
            Assert.Equal(30, recipe.CookTime);
            Assert.Equal("Easy", recipe.Difficulty);
        }

        [Fact]
        public void Ingredient_CanBeCreated_WithValidProperties()
        {
            // Arrange & Act
            var ingredient = new Ingredient
            {
                IngredientId = 1,
                IngredientName = "Tomato"
            };

            // Assert
            Assert.Equal(1, ingredient.IngredientId);
            Assert.Equal("Tomato", ingredient.IngredientName);
        }

        [Fact]
        public void DietaryRestrictions_CanBeCreated_WithValidProperties()
        {
            // Arrange & Act
            var dietary = new DietaryRestrictions
            {
                DietaryRestrictionsId = 1,
                Name = "Vegan"
            };

            // Assert
            Assert.Equal(1, dietary.DietaryRestrictionsId);
            Assert.Equal("Vegan", dietary.Name);
        }

        [Fact]
        public void Instructions_CanBeCreated_WithValidProperties()
        {
            // Arrange & Act
            var instructions = new Instructions
            {
                InstructionsId = 1,
                Instruction = "Step 1: Mix ingredients",
                RecipeId = 1
            };

            // Assert
            Assert.Equal(1, instructions.InstructionsId);
            Assert.Equal("Step 1: Mix ingredients", instructions.Instruction);
            Assert.Equal(1, instructions.RecipeId);
        }

        [Fact]
        public void Recipe_CanHaveMultipleIngredients()
        {
            // Arrange
            var recipe = new Recipe
            {
                RecipeId = 1,
                Name = "Pasta",
                Ingredients = new List<Ingredient>()
            };

            var ingredient1 = new Ingredient { IngredientId = 1, IngredientName = "Pasta" };
            var ingredient2 = new Ingredient { IngredientId = 2, IngredientName = "Tomato Sauce" };

            // Act
            recipe.Ingredients.Add(ingredient1);
            recipe.Ingredients.Add(ingredient2);

            // Assert
            Assert.Equal(2, recipe.Ingredients.Count);
            Assert.Contains(ingredient1, recipe.Ingredients);
            Assert.Contains(ingredient2, recipe.Ingredients);
        }

        [Fact]
        public void User_CanHaveMultipleSavedRecipes()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Username = "TestUser",
                Recipes = new List<Recipe>()
            };

            var recipe1 = new Recipe { RecipeId = 1, Name = "Recipe 1" };
            var recipe2 = new Recipe { RecipeId = 2, Name = "Recipe 2" };

            // Act
            user.Recipes.Add(recipe1);
            user.Recipes.Add(recipe2);

            // Assert
            Assert.Equal(2, user.Recipes.Count);
            Assert.Contains(recipe1, user.Recipes);
            Assert.Contains(recipe2, user.Recipes);
        }

        [Fact]
        public void Recipe_CookTime_ShouldBePositive()
        {
            // Arrange
            var recipe = new Recipe { CookTime = 45 };

            // Assert
            Assert.True(recipe.CookTime > 0);
        }

        [Theory]
        [InlineData("Easy")]
        [InlineData("Medium")]
        [InlineData("Hard")]
        public void Recipe_Difficulty_ShouldAcceptValidValues(string difficulty)
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                Difficulty = difficulty
            };

            // Assert
            Assert.Equal(difficulty, recipe.Difficulty);
        }
    }
}