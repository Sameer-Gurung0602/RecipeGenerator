using System;
using System.Net;
using FluentAssertions;
using Xunit;
using RecipeGenerator.DTOs;
using RecipeGenerator.Test;
using System.Net.Http.Json;
using Microsoft.Identity.Client;


namespace RecipeGenerator.test.IntegrationTests.Controllers
{
    [Collection("Database collection")]
    public class RecipeControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public RecipeControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        // GET /api/recipes
        // check if ok status is returned
        [Fact]
        public async Task GetAllRecipes_ReturnsOkStatus()
        {
            var response = await _client.GetAsync("/api/recipes");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // check list of recipes is returned
        [Fact]
        public async Task GetAllRecipes_ReturnsListOfRecipes()
        {
            var response = await _client.GetAsync("/api/recipes");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            recipes.Should().BeOfType<List<RecipeDto>>();  // should be list of RecipeDtos
            recipes.Should().HaveCount(5); // should have 5 recipes.
        }

        // check properties of each recipe
        [Fact]
        public async Task GetAllRecipes_ReturnsRecipesWithCorrectProperties()
        {
            var response = await _client.GetAsync("/api/recipes");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            foreach (RecipeDto recipe in recipes)
            {
                recipe.Name.Should().NotBeNull();
                recipe.Description.Should().NotBeNull();
                recipe.Instructions.Should().NotBeNull();
                recipe.Ingredients.Should().NotBeNull();
                recipe.CookTime.Should().BeGreaterThan(0);
                recipe.Difficulty.Should().NotBeNull();
            }
        }

        // GET /api/recipes/dietary-restrictions
        [Fact]
        public async Task GetRecipeDietaryRestrictions_ReturnsOkStatus()
        {
            var response = await _client.GetAsync("/api/recipes/23/dietary-restrictions");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetRecipeDietaryRestrictions_ReturnsListOfDietaryRestrictions()
        {
            var response = await _client.GetAsync("/api/recipes/23/dietary-restrictions");
            var restrictions = await response.Content.ReadFromJsonAsync<DietaryRestrictionsDto>();
              restrictions.DietaryRestrictions.Should().BeOfType<List<string>>();
        }

        [Fact]
        public async Task GetRecipeDietaryRestrictions_ReturnsNotFoundStatus_WhenRecipeDoesNotExist()
        {
            var response = await _client.GetAsync("/api/recipes/9999/dietary-restrictions");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // GET /api/recipes/dietary-restrictions
        [Fact]
        public async Task GetAllDietaryRestrictions_ReturnsOkStatus()
        {
            var response = await _client.GetAsync("/api/recipes/dietary-restrictions");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllDietaryRestrictions_ReturnsListOfDietaryRestrictions()
        {
            var response = await _client.GetAsync("/api/recipes/dietary-restrictions");
            var restrictions = await response.Content.ReadFromJsonAsync<DietaryRestrictionsDto>();

            restrictions.DietaryRestrictions.Should().BeOfType<List<string>>();
            restrictions.DietaryRestrictions.Should().HaveCount(3);
        }

        // POST /api/recipes/match - Recipe Matching Tests
        [Fact]
        public async Task MatchRecipes_ReturnsOkStatus()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 57 } // Shrimp, Lime, Honey
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task MatchRecipes_ReturnsBadRequest_WhenNoIngredientsProvided()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int>() // Empty list
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MatchRecipes_ReturnsListOfRecipeMatches()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 57 } // Shrimp, Honey
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().BeOfType<List<RecipeMatchDto>>();
            matchedRecipes.Should().NotBeEmpty();
        }

        [Fact]
        public async Task MatchRecipes_ReturnsCorrectMatchPercentageFor100PercentMatch()
        {
            // Arrange - Recipe 23 has ingredients: 51 (Shrimp), 56 (Ginger), 57 (Honey)
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 56, 57 }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            var recipe23 = matchedRecipes.FirstOrDefault(r => r.RecipeId == 23);
            recipe23.Should().NotBeNull();
            recipe23.MatchPercentage.Should().Be(100);
            recipe23.IngredientsMatched.Should().Be(3);
            recipe23.TotalIngredientsRequired.Should().Be(3);
            recipe23.MissingIngredients.Should().BeEmpty();
            recipe23.MatchedIngredients.Should().HaveCount(3);
        }

        [Fact]
        public async Task MatchRecipes_ReturnsCorrectMatchPercentageForPartialMatch()
        {
            // Arrange - Recipe 23 has ingredients: 51 (Shrimp), 56 (Ginger), 57 (Honey)
            // User only has 2 out of 3 ingredients
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 57 } // Missing Ginger (56)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            var recipe23 = matchedRecipes.FirstOrDefault(r => r.RecipeId == 23);
            recipe23.Should().NotBeNull();
            recipe23.MatchPercentage.Should().Be(67); // 2/3 = 66.67 rounded to 67
            recipe23.IngredientsMatched.Should().Be(2);
            recipe23.TotalIngredientsRequired.Should().Be(3);
            recipe23.MissingIngredients.Should().ContainSingle();
            recipe23.MissingIngredients.Should().Contain("Ginger");
            recipe23.MatchedIngredients.Should().HaveCount(2);
            recipe23.MatchedIngredients.Should().Contain(new[] { "Shrimp", "Honey" });
        }

        [Fact]
        public async Task MatchRecipes_ShowsAllMissingIngredients()
        {
            // Arrange - Recipe 27 has ingredients: 51 (Shrimp), 52 (Lime), 57 (Honey)
            // User only has Shrimp
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51 } // Only Shrimp
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            var recipe27 = matchedRecipes.FirstOrDefault(r => r.RecipeId == 27);
            recipe27.Should().NotBeNull();
            recipe27.MatchPercentage.Should().Be(33); // 1/3 = 33.33 rounded to 33
            recipe27.MissingIngredients.Should().HaveCount(2);
            recipe27.MissingIngredients.Should().Contain(new[] { "Lime", "Honey" });
        }

        [Fact]
        public async Task MatchRecipes_FiltersRecipesByDietaryRestrictions()
        {
            // Arrange - Recipe 23 and 27 have dietary restriction 9
            // Recipe 25 and 26 have dietary restriction 7
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 53, 54, 55, 56, 57, 58, 59, 60 }, // All ingredients
                DietaryRestrictionIds = new List<int> { 9 } // Filter by dietary restriction 9
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().NotBeEmpty();
            matchedRecipes.Should().OnlyContain(r => r.RecipeId == 23 || r.RecipeId == 27);
            matchedRecipes.Should().NotContain(r => r.RecipeId == 25 || r.RecipeId == 26);
        }

     

        [Fact]
        public async Task MatchRecipes_SortsByMatchPercentageDescendingByDefault()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 57 } // Shrimp, Lime, Honey
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().NotBeEmpty();
            matchedRecipes.Should().BeInDescendingOrder(r => r.MatchPercentage);
        }

        [Fact]
        public async Task MatchRecipes_SortsByMatchPercentageAscending()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 57 }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match?sortBy=match&sortOrder=asc", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().NotBeEmpty();
            matchedRecipes.Should().BeInAscendingOrder(r => r.MatchPercentage);
        }

        [Fact]
        public async Task MatchRecipes_SortsByCookTimeDescending()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 53, 54, 55, 56, 57, 58, 59, 60 } // All ingredients
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match?sortBy=cooktime&sortOrder=desc", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().NotBeEmpty();
            matchedRecipes.Should().BeInDescendingOrder(r => r.CookTime);
        }

        [Fact]
        public async Task MatchRecipes_SortsByDifficultyAscending()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 53, 54, 55, 56, 57, 58, 59, 60 } // All ingredients
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match?sortBy=difficulty&sortOrder=asc", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().NotBeEmpty();
            matchedRecipes.Should().BeInAscendingOrder(r => r.Difficulty);
        }

        [Fact]
        public async Task MatchRecipes_SortsByDateDescending()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 53, 54, 55, 56, 57, 58, 59, 60 } // All ingredients
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match?sortBy=date&sortOrder=desc", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().NotBeEmpty();
            matchedRecipes.Should().BeInDescendingOrder(r => r.CreatedAt);
        }

        [Fact]
        public async Task MatchRecipes_ReturnsRecipesWithCorrectProperties()
        {
            // Arrange
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 56, 57 }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            matchedRecipes.Should().NotBeEmpty();
            foreach (var recipe in matchedRecipes)
            {
                recipe.RecipeId.Should().BeGreaterThan(0);
                recipe.Name.Should().NotBeNullOrEmpty();
                recipe.Description.Should().NotBeNullOrEmpty();
                recipe.CookTime.Should().BeGreaterThan(0);
                recipe.Difficulty.Should().NotBeNullOrEmpty();
                recipe.MatchPercentage.Should().BeInRange(0, 100);
                recipe.TotalIngredientsRequired.Should().BeGreaterThan(0);
                recipe.IngredientsMatched.Should().BeInRange(0, recipe.TotalIngredientsRequired);
                recipe.MatchedIngredients.Should().NotBeNull();
                recipe.MissingIngredients.Should().NotBeNull();
                
                // Verify totals add up
                (recipe.MatchedIngredients.Count + recipe.MissingIngredients.Count)
                    .Should().Be(recipe.TotalIngredientsRequired);
            }
        }

        [Fact]
        public async Task MatchRecipes_ReturnsEmptyList_WhenNoRecipesMatchCriteria()
        {
            // Arrange - Request with impossible combination
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51 }, // Only one ingredient
                DietaryRestrictionIds = new List<int> { 99 } // Non-existent dietary restriction
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            matchedRecipes.Should().BeEmpty();
        }

        [Fact]
        public async Task MatchRecipes_HandlesMultipleDietaryRestrictionsCorrectly()
        {
            // Arrange - Recipe must have ALL specified dietary restrictions
            var request = new RecipeMatchRequestDto
            {
                IngredientIds = new List<int> { 51, 52, 53, 54, 55, 56, 57, 58, 59, 60 }, // All ingredients
                DietaryRestrictionIds = new List<int> { 7, 9 } // Must have BOTH restrictions
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/recipes/match", request);
            var matchedRecipes = await response.Content.ReadFromJsonAsync<List<RecipeMatchDto>>();

            // Assert
            // Based on test data, no recipe has both restrictions 7 AND 9
            matchedRecipes.Should().BeEmpty();
        }
    }
}
