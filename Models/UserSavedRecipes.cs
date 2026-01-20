namespace RecipeGenerator.Models
{
    public class UserSavedRecipes
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
