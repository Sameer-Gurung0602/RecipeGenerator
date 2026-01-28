using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RecipeGenerator.DTOs;
using RecipeGenerator.Test;
using Xunit;

namespace RecipeGenerator.test.IntegrationTests.Controllers
{
    [Collection("Database collection")]
    public class FavouritesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        // Single user application - userId is 9 based on test data
        private const int SingleUserId = 9;

        public FavouritesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        // GET /api/favourites/{userId}
        [Fact]
        public async Task GetAllFavourites_ReturnsOkStatus()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllFavourites_ReturnsListOfRecipes()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeNull();
            favourites.Should().BeOfType<List<RecipeDto>>();
            favourites.Should().AllBeAssignableTo<RecipeDto>();
        }

        [Fact]
        public async Task GetAllFavourites_ReturnsCorrectNumberOfFavourites()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().HaveCount(5, 
                "UserSavedRecipes.json contains 5 recipes for userId 9: RecipeIds 23, 24, 25, 26, 27");
        }

        [Fact]
        public async Task GetAllFavourites_ReturnsCorrectRecipeIds()
        {
            // Arrange
            var expectedRecipeIds = new[] { 23, 24, 25, 26, 27 }; // From UserSavedRecipes.json

            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            var actualRecipeIds = favourites.Select(r => r.RecipeId).OrderBy(id => id).ToArray();
            actualRecipeIds.Should().BeEquivalentTo(expectedRecipeIds,
                "these are the saved recipe IDs for userId 9 in UserSavedRecipes.json");
        }

        [Fact]
        public async Task GetAllFavourites_ReturnsUniqueRecipeIds()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeEmpty();
            favourites.Select(r => r.RecipeId).Should().OnlyHaveUniqueItems(
                "each recipe should only appear once in favourites");
        }

        [Fact]
        public async Task GetAllFavourites_ReturnsRecipesWithAllRequiredProperties()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeEmpty();
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.RecipeId.Should().BeGreaterThan(0);
                recipe.Name.Should().NotBeNullOrEmpty();
                recipe.Description.Should().NotBeNullOrEmpty();
                recipe.CookTime.Should().BeGreaterThan(0);
                recipe.Difficulty.Should().NotBeNullOrEmpty();
                recipe.CreatedAt.Should().NotBe(default(DateTime));
                recipe.Img.Should().NotBeNullOrEmpty(); // ✅ Add this
                recipe.Instructions.Should().NotBeNull();
                recipe.Ingredients.Should().NotBeNull();
                recipe.DietaryRestrictions.Should().NotBeNull();
            });
        }

        [Fact]
        public async Task GetAllFavourites_IncludesInstructionsWithCorrectProperties()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeEmpty();
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.Instructions.Should().NotBeNull("Instructions navigation property must be loaded");
                recipe.Instructions.InstructionsId.Should().BeGreaterThan(0);
                recipe.Instructions.Instruction.Should().NotBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task GetAllFavourites_IncludesIngredientsList()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeEmpty();
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.Ingredients.Should().NotBeNull("Ingredients navigation property must be loaded");
                recipe.Ingredients.Should().AllBeOfType<string>();
                recipe.Ingredients.Should().AllSatisfy(ingredient =>
                {
                    ingredient.Should().NotBeNullOrWhiteSpace();
                });
            });
        }

        [Fact]
        public async Task GetAllFavourites_IncludesDietaryRestrictionsList()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeEmpty();
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.DietaryRestrictions.Should().NotBeNull(
                    "DietaryRestrictions should be loaded as empty list if none exist");
                recipe.DietaryRestrictions.Should().AllBeOfType<string>();
            });
        }

        [Fact]
        public async Task GetAllFavourites_DoesNotThrowNullReferenceException()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert - Should complete without errors
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            favourites.Should().NotBeNull();
            
            // Verify fix for NullReferenceException by accessing all navigation properties
            var exception = Record.Exception(() =>
            {
                foreach (var recipe in favourites)
                {
                    _ = recipe.Instructions.InstructionsId;
                    _ = recipe.Instructions.Instruction;
                    _ = recipe.DietaryRestrictions.Count;
                    _ = recipe.Ingredients.Count;
                }
            });
            
            exception.Should().BeNull(
                "all navigation properties should be loaded via .Include() statements");
        }

        [Fact]
        public async Task GetAllFavourites_VerifyAllNavigationPropertiesAreEagerLoaded()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeEmpty();
            
            // Verify Instructions navigation property
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.Instructions.Should().NotBeNull(
                    "Instructions should be loaded via .Include(r => r.Instructions)");
                recipe.Instructions.InstructionsId.Should().BeGreaterThan(0);
            });
            
            // Verify Ingredients navigation property
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.Ingredients.Should().NotBeNull(
                    "Ingredients should be loaded via .Include(r => r.Ingredients)");
            });
            
            // Verify DietaryRestrictions navigation property
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.DietaryRestrictions.Should().NotBeNull(
                    "DietaryRestrictions should be loaded via .Include(r => r.DietaryRestrictions)");
            });
        }

        [Fact]
        public async Task GetAllFavourites_ReturnsRecipesWithImageUrls()
        {
            // Act
            var response = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var favourites = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            favourites.Should().NotBeEmpty();
            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.Img.Should().NotBeNullOrEmpty("all recipes should have an image URL");
                recipe.Img.Should().StartWith("https://", "image URLs should be valid HTTPS URLs");
            });
        }
    }
}