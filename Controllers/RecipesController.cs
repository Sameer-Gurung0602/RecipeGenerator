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
        public async Task<IActionResult> GetAllRecipes([FromQuery] int userId = 1, [FromQuery] string? sortBy = null, [FromQuery] string? sortOrder = "asc")
        {
            var recipes = await _recipeService.GetAllRecipes(userId, sortBy, sortOrder);
            return Ok(recipes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(int id, [FromQuery] int userId = 1)
        {
            var recipe = await _recipeService.GetRecipeById(id, userId);
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
            [FromQuery] int userId = 1,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            try
            {
                // Validate input - require at least one search criteria
                var hasIngredients = request.IngredientIds != null && request.IngredientIds.Any();
                var hasDietary = request.DietaryRestrictionIds != null && request.DietaryRestrictionIds.Any();

                if (!hasIngredients && !hasDietary)
                {
                    return BadRequest("At least one ingredient or dietary restriction is required.");
                }

                var recipes = await _recipeService.GetMatchingRecipes(
                    userId,
                    request.IngredientIds,  // ✅ CHANGED: Pass null as-is, DON'T convert to empty list
                    request.DietaryRestrictionIds,
                    sortBy,
                    sortOrder);

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                // Temporary error logging
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetAllIngredients()
        {
            var ingredients = await _recipeService.GetAllIngredients();
            return Ok(ingredients);
        }
    }
}
