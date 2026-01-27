using Microsoft.AspNetCore.Mvc;
using RecipeGenerator.Models;
using RecipeGenerator.Services;
using RecipeGenerator.DTOs;

namespace RecipeGenerator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class RecipesController : ControllerBase
    {
        private readonly RecipeService _recipeService;

        public RecipesController(RecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRecipes([FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
        {
            var recipes = await _recipeService.GetAllRecipes(sortBy, sortOrder);
            return Ok(recipes);
        }
    
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            var recipe = await _recipeService.GetRecipeById(id);
            
            if (recipe == null)
            {
                return NotFound(new { message = $"Recipe with ID {id} not found." });
            }
            
            return Ok(recipe);
        }

        [HttpGet("{id}/dietary-restrictions")]
        public async Task<IActionResult> GetRecipeDietaryRestrictions(int id)
        {
            var dietaryRestrictions = await _recipeService.GetRecipeDietaryRestrictions(id);
            
            if (dietaryRestrictions == null)
            {
                return NotFound(new { message = "Dietary restrictions not found." });
            }
            
            return Ok(dietaryRestrictions);
        }

        [HttpGet("dietary-restrictions")]
        public async Task<IActionResult> GetDietaryRestrictions()
        {
            var dietaryRestrictions = await _recipeService.GetDietaryRestrictions();
            return Ok(dietaryRestrictions);
        }

        [HttpPost("match")]
        public async Task<IActionResult> GetRecipesByIngredients(
            [FromBody] RecipeMatchRequestDto request,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            // Validate input
            if (request.IngredientIds == null || !request.IngredientIds.Any())
            {
                return BadRequest("At least one ingredient is required.");
            }

            var recipes = await _recipeService.GetMatchingRecipes(
                request.IngredientIds,
                request.DietaryRestrictionIds,
                sortBy,
                sortOrder);
            
            return Ok(recipes);
        }
    }
}
