using RecipeGenerator.Models;

namespace RecipeGenerator.Models
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public ICollection<Recipe> Recipes { get; set; }
    }
}
