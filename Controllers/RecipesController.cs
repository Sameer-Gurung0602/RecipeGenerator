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
    }
}
