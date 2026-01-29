using FluentAssertions;
using RecipeGenerator.DTOs;
using RecipeGenerator.Test;
using RecipeGenerator.Test.Helpers;
using RecipeGenerator.Data;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace RecipeGenerator.test.IntegrationTests.Controllers
{
    [Collection("Database collection")]
    public class FavouritesControllerTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private const int SingleUserId = 9;

        public FavouritesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RecipeGeneratorDbContext>();

            Console.WriteLine("?? Resetting UserRecipes for test isolation...");

            await db.Database.ExecuteSqlRawAsync("DELETE FROM UserRecipes WHERE UserId = {0}", SingleUserId);

            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO UserRecipes (UserId, RecipeId) VALUES ({0}, {1})", SingleUserId, 23);
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO UserRecipes (UserId, RecipeId) VALUES ({0}, {1})", SingleUserId, 24);
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO UserRecipes (UserId, RecipeId) VALUES ({0}, {1})", SingleUserId, 25);
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO UserRecipes (UserId, RecipeId) VALUES ({0}, {1})", SingleUserId, 26);
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO UserRecipes (UserId, RecipeId) VALUES ({0}, {1})", SingleUserId, 27);

            Console.WriteLine("? UserRecipes reset complete - 5 favourites restored");
        }

        public Task DisposeAsync() => Task.CompletedTask;

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
            var expectedRecipeIds = new[] { 23, 24, 25, 26, 27 };

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
                recipe.Img.Should().NotBeNullOrEmpty();
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

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            favourites.Should().NotBeNull();

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

            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.Instructions.Should().NotBeNull(
                    "Instructions should be loaded via .Include(r => r.Instructions)");
                recipe.Instructions.InstructionsId.Should().BeGreaterThan(0);
            });

            favourites.Should().AllSatisfy(recipe =>
            {
                recipe.Ingredients.Should().NotBeNull(
                    "Ingredients should be loaded via .Include(r => r.Ingredients)");
            });

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

        // POST /api/favourites/{recipeId} - Add to Favourites Tests



        // DELETE /api/favourites/{recipeId} - Remove from Favourites Tests
        [Fact]
        public async Task RemoveFavouriteRecipe_ReturnsOkStatus_WhenRemovedSuccessfully()
        {
            // Arrange
            int existingFavouriteId = 23;

            // Act
            var response = await _client.DeleteAsync($"/api/favourites/{existingFavouriteId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RemoveFavouriteRecipe_RemovesRecipeFromFavourites()
        {
            // Arrange
            int existingFavouriteId = 24;

            // Get current count
            var beforeResponse = await _client.GetAsync($"/api/favourites/{SingleUserId}");
            var beforeFavourites = await beforeResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();
            int countBefore = beforeFavourites.Count;

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/favourites/{existingFavouriteId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var afterResponse = await _client.GetAsync($"/api/favourites/{SingleUserId}");             //Verify count decrease
            var afterFavourites = await afterResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();
            afterFavourites.Should().HaveCount(countBefore - 1, "one recipe should be removed");
            afterFavourites.Should().NotContain(r => r.RecipeId == existingFavouriteId,
                "the removed recipe should not be in favourites");
        }

        [Fact]
        public async Task RemoveFavouriteRecipe_ReturnsNotFound_WhenRecipeNotInFavourites()
        {
            // Arrange
            int notFavouritedRecipeId = 28;  //Valid recipe that is not favourited

            // Act
            var response = await _client.DeleteAsync($"/api/favourites/{notFavouritedRecipeId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RemoveFavouriteRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
        {
            // Arrange
            int nonExistentRecipeId = 99999;

            // Act
            var response = await _client.DeleteAsync($"/api/favourites/{nonExistentRecipeId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RemoveFavouriteRecipe_PersistsInDatabase()
        {
            // Arrange
            int existingFavouriteId = 25;

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/favourites/{existingFavouriteId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert
            var getResponse = await _client.GetAsync($"/api/favourites/{SingleUserId}"); //check by refetching favourites
            var favourites = await getResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();

            favourites.Should().NotContain(r => r.RecipeId == existingFavouriteId,
                "the removed recipe should not persist in DB");
        }
    }
}