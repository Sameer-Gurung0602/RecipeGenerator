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

        //[HttpPost("{id}")]



        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveRecipe(int id)
        {
            var result = await _favouritesService.RemoveFavourite(id);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    "NotFound" => NotFound(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            return Ok(new { message = "Recipe removed from favourites successfully" });
        }
    }
}
