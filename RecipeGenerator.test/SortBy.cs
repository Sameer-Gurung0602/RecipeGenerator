using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RecipeGenerator.DTOs;
using RecipeGenerator.Test;
using Xunit;

namespace RecipeGenerator.test
{
    [Collection("Database collection")]
    public class SortBy : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
    
        public SortBy(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

 
        [Fact]
        public async Task GetAllRecipes_SortByDate_Ascending_ReturnsRecipesOrderedByDateAscending()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=date&sortOrder=asc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Verify ascending order
            recipes.Should().BeInAscendingOrder(r => r.CreatedAt);
            recipes![0].Name.Should().Be("Honey Ginger Shrimp"); // 2025-01-23
            recipes![4].Name.Should().Be("Honey Lime Shrimp Bowl"); // 2025-01-27
        }

        [Fact]
        public async Task GetAllRecipes_SortByDate_Descending_ReturnsRecipesOrderedByDateDescending()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=date&sortOrder=desc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Verify descending order
            recipes.Should().BeInDescendingOrder(r => r.CreatedAt);
            recipes![0].Name.Should().Be("Honey Lime Shrimp Bowl"); // 2025-01-27
            recipes![4].Name.Should().Be("Honey Ginger Shrimp"); // 2025-01-23
        }

   

        [Fact]
        public async Task GetAllRecipes_SortByCookTime_Ascending_ReturnsRecipesOrderedByCookTimeAscending()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=cooktime&sortOrder=asc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Verify ascending order
            recipes.Should().BeInAscendingOrder(r => r.CookTime);
            recipes![0].CookTime.Should().Be(15); // Broccoli Soy Bowl
            recipes![0].Name.Should().Be("Broccoli Soy Bowl");
        }

        [Fact]
        public async Task GetAllRecipes_SortByCookTime_Descending_ReturnsRecipesOrderedByCookTimeDescending()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=cooktime&sortOrder=desc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Verify descending order
            recipes.Should().BeInDescendingOrder(r => r.CookTime);
            recipes![0].CookTime.Should().Be(30); // Cilantro Lime Tacos
            recipes![0].Name.Should().Be("Cilantro Lime Tacos");
        }
 

        [Fact]
        public async Task GetAllRecipes_SortByDifficulty_Ascending_ReturnsRecipesOrderedByDifficultyAscending()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=difficulty&sortOrder=asc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Verify ascending order (Easy comes before Medium alphabetically)
            recipes.Should().BeInAscendingOrder(r => r.Difficulty);
            
            // First 3 should be "Easy"
            recipes!.Take(3).Should().AllSatisfy(r => r.Difficulty.Should().Be("Easy"));
            
            // Last 2 should be "Medium"
            recipes!.Skip(3).Should().AllSatisfy(r => r.Difficulty.Should().Be("Medium"));
        }

        [Fact]
        public async Task GetAllRecipes_SortByDifficulty_Descending_ReturnsRecipesOrderedByDifficultyDescending()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=difficulty&sortOrder=desc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Verify descending order (Medium comes before Easy alphabetically)
            recipes.Should().BeInDescendingOrder(r => r.Difficulty);
            
            // First 2 should be "Medium"
            recipes!.Take(2).Should().AllSatisfy(r => r.Difficulty.Should().Be("Medium"));
            
            // Last 3 should be "Easy"
            recipes!.Skip(2).Should().AllSatisfy(r => r.Difficulty.Should().Be("Easy"));
        }

 

        [Fact]
        public async Task GetAllRecipes_NoSortParameters_ReturnsRecipesInDefaultOrder()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Default order is by RecipeId (ascending)
            recipes.Should().BeInAscendingOrder(r => r.RecipeId);
        }

        [Fact]
        public async Task GetAllRecipes_InvalidSortBy_ReturnsRecipesInDefaultOrder()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=invalid&sortOrder=asc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Should fall back to default order (RecipeId)
            recipes.Should().BeInAscendingOrder(r => r.RecipeId);
        }

        [Fact]
        public async Task GetAllRecipes_OnlySortByParameter_UsesAscendingAsDefault()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=cooktime");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().HaveCount(5);
            
            // Should default to ascending
            recipes.Should().BeInAscendingOrder(r => r.CookTime);
        }

 

        [Fact]
        public async Task GetAllRecipes_SortByUpperCase_WorksCorrectly()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=DATE&sortOrder=ASC");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().BeInAscendingOrder(r => r.CreatedAt);
        }

        [Fact]
        public async Task GetAllRecipes_SortByMixedCase_WorksCorrectly()
        {
            // Act
            var response = await _client.GetAsync("/api/recipes?sortBy=CookTime&sortOrder=Desc");
            var recipes = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            recipes.Should().NotBeNull();
            recipes.Should().BeInDescendingOrder(r => r.CookTime);
        }

        
    }
}
