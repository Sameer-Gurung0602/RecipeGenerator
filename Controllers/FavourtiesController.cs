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
        public async Task < IActionResult> GetSavedRecipes(int id)
        {
            var recipes =  await _favouritesService.GetAllFavourites(id);
            return Ok(recipes);
        }

        [HttpPost("{id}")]
        public IActionResult SaveRecipe(int  id)
        {


            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult RemoveRecipe(int id)
        {
            return Ok(); }

    }
}
