using System;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RecipeGenerator.DTOs;
using RecipeGenerator.Test;

namespace RecipeGenerator.test.UnitTests
{
    [Collection("Database collection")]
    public class GetRecipeById : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public GetRecipeById(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetRecipeById_WithValidId_ReturnsOkStatus()
        {
            // Arrange
            int validRecipeId = 23; // From test data: "Honey Ginger Shrimp"

            // Act
            var response = await _client.GetAsync($"/api/recipes/{validRecipeId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetRecipeById_WithValidId_ReturnsCorrectRecipe()
        {
            // Arrange
            int validRecipeId = 23; // "Honey Ginger Shrimp"

            // Act
            var response = await _client.GetAsync($"/api/recipes/{validRecipeId}");
            var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();

            // Assert
            recipe.Should().NotBeNull();
            recipe!.RecipeId.Should().Be(validRecipeId);
            recipe.Name.Should().Be("Honey Ginger Shrimp");
            recipe.Description.Should().Be("Sweet and spicy shrimp with ginger");
            recipe.CookTime.Should().Be(20);
            recipe.Difficulty.Should().Be("Easy");
            recipe.CreatedAt.Should().Be(new DateTime(2025, 1, 23));
        }

        [Fact]
        public async Task GetRecipeById_WithValidId_ReturnsRecipeWithInstructions()
        {
            // Arrange
            int validRecipeId = 24; // "Pork Cabbage Stir Fry"

            // Act
            var response = await _client.GetAsync($"/api/recipes/{validRecipeId}");
            var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();

            // Assert
            recipe.Should().NotBeNull();
            recipe!.Instructions.Should().NotBeNull();
            recipe.Instructions!.InstructionsId.Should().Be(24);
            recipe.Instructions.Instruction.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetRecipeById_WithValidId_ReturnsRecipeWithIngredients()
        {
            // Arrange
            int validRecipeId = 23; // "Honey Ginger Shrimp"

            // Act
            var response = await _client.GetAsync($"/api/recipes/{validRecipeId}");
            var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();

            // Assert
            recipe.Should().NotBeNull();
            recipe!.Ingredients.Should().NotBeNull();
            recipe.Ingredients.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetRecipeById_WithValidId_ReturnsRecipeWithDietaryRestrictions()
        {
            // Arrange
            int validRecipeId = 25; // "Cilantro Lime Tacos"

            // Act
            var response = await _client.GetAsync($"/api/recipes/{validRecipeId}");
            var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();

            // Assert
            recipe.Should().NotBeNull();
            recipe!.DietaryRestrictions.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRecipeById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidRecipeId = 99999;

            // Act
            var response = await _client.GetAsync($"/api/recipes/{invalidRecipeId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetRecipeById_WithInvalidId_ReturnsErrorMessage()
        {
            // Arrange
            int invalidRecipeId = 99999;

            // Act
            var response = await _client.GetAsync($"/api/recipes/{invalidRecipeId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            content.Should().Contain("not found");
        }

        [Theory]
        [InlineData(23, "Honey Ginger Shrimp")]
        [InlineData(24, "Pork Cabbage Stir Fry")]
        [InlineData(25, "Cilantro Lime Tacos")]
        [InlineData(26, "Broccoli Soy Bowl")]
        [InlineData(27, "Honey Lime Shrimp Bowl")]
        public async Task GetRecipeById_WithMultipleValidIds_ReturnsCorrectRecipes(int recipeId, string expectedName)
        {
            // Act
            var response = await _client.GetAsync($"/api/recipes/{recipeId}");
            var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipe.Should().NotBeNull();
            recipe!.RecipeId.Should().Be(recipeId);
            recipe.Name.Should().Be(expectedName);
        }

        [Fact]
        public async Task GetRecipeById_WithZeroId_ReturnsNotFound()
        {
            // Arrange
            int zeroId = 0;

            // Act
            var response = await _client.GetAsync($"/api/recipes/{zeroId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetRecipeById_WithNegativeId_ReturnsNotFound()
        {
            // Arrange
            int negativeId = -1;

            // Act
            var response = await _client.GetAsync($"/api/recipes/{negativeId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetRecipeById_ReturnsRecipeDto_WithAllRequiredProperties()
        {
            // Arrange
            int validRecipeId = 23;

            // Act
            var response = await _client.GetAsync($"/api/recipes/{validRecipeId}");
            var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();

            // Assert
            recipe.Should().NotBeNull();
            recipe!.RecipeId.Should().BeGreaterThan(0);
            recipe.Name.Should().NotBeNullOrEmpty();
            recipe.Description.Should().NotBeNullOrEmpty();
            recipe.CookTime.Should().BeGreaterThan(0);
            recipe.Difficulty.Should().NotBeNullOrEmpty();
            recipe.CreatedAt.Should().NotBe(default(DateTime));
            recipe.Ingredients.Should().NotBeNull();
            recipe.DietaryRestrictions.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRecipeById_ReturnsBadRequest()
        {   //Arrange
            string invalidRecipeId = "Cows";
            //Act
            var response = await _client.GetAsync($"/api/recipes/{invalidRecipeId}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);


        }
    }
}
