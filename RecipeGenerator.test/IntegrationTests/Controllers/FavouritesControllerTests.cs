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
            favourites.Should().NotBeNull();
            favourites!.Count.Should().BeGreaterThanOrEqualTo(5, 
                "UserSavedRecipes.json contains at least 5 recipes for userId 9: RecipeIds 23, 24, 25, 26, 27");
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
            favourites.Should().NotBeNull();
            var actualRecipeIds = favourites!.Select(r => r.RecipeId).OrderBy(id => id).ToArray();
            actualRecipeIds.Should().Contain(expectedRecipeIds, 
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
            favourites!.Select(r => r.RecipeId).Should().OnlyHaveUniqueItems(
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
            favourites!.Should().AllSatisfy(recipe =>
            {
                recipe.RecipeId.Should().BeGreaterThan(0);
                recipe.Name.Should().NotBeNullOrEmpty();
                recipe.Description.Should().NotBeNullOrEmpty();
                recipe.CookTime.Should().BeGreaterThan(0);
                recipe.Difficulty.Should().NotBeNullOrEmpty();
                recipe.CreatedAt.Should().NotBe(default(DateTime));
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
            favourites!.Should().AllSatisfy(recipe =>
            {
                recipe.Instructions.Should().NotBeNull("Instructions navigation property must be loaded");
                recipe.Instructions!.InstructionsId.Should().BeGreaterThan(0);
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
            favourites!.Should().AllSatisfy(recipe =>
            {
                recipe.Ingredients.Should().NotBeNull("Ingredients navigation property must be loaded");
                recipe.Ingredients!.Should().AllBeOfType<string>();
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
            favourites!.Should().AllSatisfy(recipe =>
            {
                recipe.DietaryRestrictions.Should().NotBeNull(
                    "DietaryRestrictions should be loaded as empty list if none exist");
                recipe.DietaryRestrictions!.Should().AllBeOfType<string>();
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
                foreach (var recipe in favourites!)
                {
                    _ = recipe.Instructions!.InstructionsId;
                    _ = recipe.Instructions.Instruction;
                    _ = recipe.DietaryRestrictions!.Count;
                    _ = recipe.Ingredients!.Count;
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
            favourites!.Should().AllSatisfy(recipe =>
            {
                recipe.Instructions.Should().NotBeNull(
                    "Instructions should be loaded via .Include(r => r.Instructions)");
                recipe.Instructions!.InstructionsId.Should().BeGreaterThan(0);
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

        // POST /api/favourites/{userId}/recipes - Save Recipe to Favourites Tests
        
        [Fact]
        public async Task SaveRecipe_WithValidData_ReturnsOkStatus()
        {
            // Arrange - User 10 has NO favorites, adding recipe 23
            var userId = 10;
            var request = new SaveRecipeDTO
            {
                RecipeId = 23
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SaveRecipe_WithValidData_ReturnsSuccessMessage()
        {
            // Arrange - User 10 adding recipe 24
            var userId = 10;
            var request = new SaveRecipeDTO
            {
                RecipeId = 24
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Recipe saved to favourites successfully");
        }

        [Fact]
        public async Task SaveRecipe_WithInvalidRecipeId_ReturnsNotFound()
        {
            // Arrange
            var userId = 9;
            var request = new SaveRecipeDTO
            {
                RecipeId = 99999 // Non-existent recipe
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SaveRecipe_WithInvalidUserId_ReturnsNotFound()
        {
            // Arrange
            var userId = 99999; // Non-existent user
            var request = new SaveRecipeDTO
            {
                RecipeId = 23
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SaveRecipe_WithZeroRecipeId_ReturnsBadRequest()
        {
            // Arrange
            var userId = 9;
            var request = new SaveRecipeDTO
            {
                RecipeId = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SaveRecipe_WithNegativeRecipeId_ReturnsBadRequest()
        {
            // Arrange
            var userId = 9;
            var request = new SaveRecipeDTO
            {
                RecipeId = -1
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SaveRecipe_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            var userId = 9;

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", (SaveRecipeDTO?)null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SaveRecipe_AlreadyFavourited_ReturnsNotFound()
        {
            // Arrange - Recipe 23 is already favourited by user 9 in seed data
            var userId = 9;
            var request = new SaveRecipeDTO
            {
                RecipeId = 23 // Already in user 9's favourites
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("already favourited");
        }

        [Fact]
        public async Task SaveRecipe_IncreasesUserFavouritesCount()
        {
            // Arrange - User 11 has NO favorites, adding recipe 23
            var userId = 11;
            var recipeIdToAdd = 23;
            
            // Get initial count
            var initialResponse = await _client.GetAsync($"/api/favourites/{userId}");
            var initialFavourites = await initialResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();
            var initialCount = initialFavourites?.Count ?? 0;

            var request = new SaveRecipeDTO
            {
                RecipeId = recipeIdToAdd
            };

            // Act
            var saveResponse = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);
            
            // Get updated count
            var updatedResponse = await _client.GetAsync($"/api/favourites/{userId}");
            var updatedFavourites = await updatedResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            saveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedFavourites.Should().HaveCount(initialCount + 1);
            updatedFavourites!.Should().Contain(r => r.RecipeId == recipeIdToAdd);
        }

        [Fact]
        public async Task SaveRecipe_AddsCorrectRecipeToFavourites()
        {
            // Arrange - User 11 adding recipe 24
            var userId = 11;
            var recipeIdToAdd = 24;
            var request = new SaveRecipeDTO
            {
                RecipeId = recipeIdToAdd
            };

            // Act
            var saveResponse = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);
            var getFavouritesResponse = await _client.GetAsync($"/api/favourites/{userId}");
            var favourites = await getFavouritesResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            saveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            favourites!.Should().Contain(r => r.RecipeId == recipeIdToAdd,
                "the saved recipe should appear in the user's favourites");
        }

        [Fact]
        public async Task SaveRecipe_WithValidData_RecipeAppearsInGetFavourites()
        {
            // Arrange - User 11 adding recipe 25
            var userId = 11;
            var recipeIdToAdd = 25;
            var request = new SaveRecipeDTO
            {
                RecipeId = recipeIdToAdd
            };

            // Act - Save the recipe
            var saveResponse = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);

            // Get the favourites
            var getFavouritesResponse = await _client.GetAsync($"/api/favourites/{userId}");
            var favourites = await getFavouritesResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();
            var savedRecipe = favourites?.FirstOrDefault(r => r.RecipeId == recipeIdToAdd);

            // Assert
            saveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            savedRecipe.Should().NotBeNull("the saved recipe should be retrievable");
            savedRecipe!.RecipeId.Should().Be(recipeIdToAdd);
            savedRecipe.Name.Should().NotBeNullOrEmpty();
            savedRecipe.Ingredients.Should().NotBeEmpty();
            savedRecipe.Instructions.Should().NotBeNull();
        }

        [Fact]
        public async Task SaveRecipe_MultipleRecipes_AllAppearInFavourites()
        {
            // Arrange - User 12 has NO favorites, adding recipes 23, 24, 25
            var userId = 12;
            var recipeIds = new[] { 23, 24, 25 };

            // Act - Save multiple recipes
            foreach (var recipeId in recipeIds)
            {
                var request = new SaveRecipeDTO { RecipeId = recipeId };
                var response = await _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            // Get all favourites
            var getFavouritesResponse = await _client.GetAsync($"/api/favourites/{userId}");
            var favourites = await getFavouritesResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();
            var favouriteIds = favourites?.Select(r => r.RecipeId).ToArray() ?? Array.Empty<int>();

            // Assert
            favouriteIds.Should().Contain(recipeIds, "all saved recipes should appear in favourites");
        }

        [Fact]
        public async Task SaveRecipe_ConcurrentRequests_HandlesGracefully()
        {
            // Arrange - User 10 adding recipe 25 concurrently
            var userId = 10;
            var recipeId = 25;
            var request = new SaveRecipeDTO { RecipeId = recipeId };

            // Act - Try to save the same recipe twice concurrently
            var task1 = _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);
            var task2 = _client.PostAsJsonAsync($"/api/favourites/{userId}/recipes", request);
            
            var responses = await Task.WhenAll(task1, task2);

            // Assert - At least one should succeed, verify no duplicates in database
            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var notFoundCount = responses.Count(r => r.StatusCode == HttpStatusCode.NotFound);

            // Either one succeeds and one fails (ideal), or both succeed but DB constraint prevents duplicate
            (successCount >= 1).Should().BeTrue("at least one request should succeed");
            
            // Verify the recipe only appears once in favourites (most important check)
            var getFavouritesResponse = await _client.GetAsync($"/api/favourites/{userId}");
            var favourites = await getFavouritesResponse.Content.ReadFromJsonAsync<List<RecipeDto>>();
            var recipeCount = favourites?.Count(f => f.RecipeId == recipeId) ?? 0;
            
            recipeCount.Should().Be(1, "recipe should only appear once in favourites despite concurrent requests");
        }
    }
}