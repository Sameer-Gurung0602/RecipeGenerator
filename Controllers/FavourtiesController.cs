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

        [HttpPost("{id}/recipes")]
        public async Task<IActionResult> SaveRecipe(int userId, [FromBody] int recipeId)
        {
             var recipe = await _favouritesService.SaveRecipe(userId, recipeId);

            return Ok();


        }

        

    }
}
   
