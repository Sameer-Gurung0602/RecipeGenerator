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


        [HttpGet("{id}")]
        public async Task<IActionResult> GetSavedRecipes(int id)
        {
            var recipes = await _favouritesService.GetAllFavourites(id);
            return Ok(recipes);
        }

        [HttpPost("{id}/recipes")]
        public async Task<IActionResult> SaveRecipe(int userId, [FromBody] SaveRecipeDTO request
            )
        {
             if(request == null || request.RecipeId <= 0)
            { return BadRequest("Invalid request data.");
             }

             var success = await _favouritesService.SaveRecipe(userId, request.RecipeId);
             if(!success)
            { return NotFound("Recipe or User Not Found or recipe is already favourited");
              };
              return Ok("Recipe saved to favourites successfully.");


        }
}
   
