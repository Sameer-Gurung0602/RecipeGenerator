using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSavedRecipes(int id)
        {
            var recipes = await _favouritesService.GetAllFavourites(id);
            return Ok(recipes);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveRecipe(int id)
        {
            var success = await _favouritesService.RemoveFavourite(id);

            if (!success)
            {
                return NotFound(new { message = $"Recipe with ID {id} not found or not in favourites." });
            }

            return Ok(new { message = "Recipe removed from favourites successfully" });
        }
    }
}
