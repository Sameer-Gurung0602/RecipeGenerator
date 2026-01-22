using RecipeGenerator.Models;

namespace RecipeGenerator.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public ICollection<Recipe> Recipes { get; set; }
    }
}
