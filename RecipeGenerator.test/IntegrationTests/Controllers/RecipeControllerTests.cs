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
    }
}
