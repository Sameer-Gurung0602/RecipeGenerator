using Microsoft.AspNetCore.Mvc;
using RecipeGenerator.Models;
using RecipeGenerator.Services;

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
        public async Task<IActionResult> GetAllRecipes()
        {
            var recipes = await _recipeService.GetAllRecipes();
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
    }
}
