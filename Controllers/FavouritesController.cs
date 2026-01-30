using Microsoft.AspNetCore.Mvc;
using RecipeGenerator.DTOs;
using RecipeGenerator.Services;

namespace RecipeGenerator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Favourites : Controller
    {
        private readonly FavouritesService _favouritesService;

        public Favourites(FavouritesService favouritesService)
        {
            _favouritesService = favouritesService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSavedRecipes(int userId)
        {
            var recipes = await _favouritesService.GetAllFavourites(userId);
            return Ok(recipes);
        }

        [HttpPost("{userId}/recipes")]
        public async Task<IActionResult> SaveRecipe(int userId, [FromBody] SaveRecipeDTO request)
        {
            if (request == null || request.RecipeId <= 0)
            {
                return BadRequest("Invalid request data.");
            }

            var success = await _favouritesService.SaveRecipe(userId, request.RecipeId);
            if (!success)
            {
                return NotFound("Recipe or User Not Found or recipe is already favourited");
            }

            return Ok("Recipe saved to favourites successfully.");
        }
        [HttpDelete("{userId}/recipes/{recipeId}")]
        public async Task<IActionResult> RemoveRecipe(int userId, int recipeId)
        {
            var success = await _favouritesService.RemoveFavourite(userId, recipeId);

            if (!success)
            {
                return NotFound(new { message = $"Recipe with ID {recipeId} not found or not in user's favourites." });
            }

            return Ok(new { message = "Recipe removed from favourites successfully" });
        }
    }
}