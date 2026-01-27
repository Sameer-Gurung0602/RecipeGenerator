using Microsoft.AspNetCore.Mvc;

namespace RecipeGenerator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Favourites : Controller
    {
    

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetSavedRecipes()
        {
            return Ok();
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
