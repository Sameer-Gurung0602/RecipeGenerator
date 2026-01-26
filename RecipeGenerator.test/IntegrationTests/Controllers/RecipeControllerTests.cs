using System;
using System.Net;
using FluentAssertions;
using Xunit;
using RecipeGenerator.DTOs;
using RecipeGenerator.Test;
using System.Net.Http.Json;


namespace RecipeGenerator.test.IntegrationTests.Controllers
{
    public class RecipeControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public RecipeControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

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
    }
}
