namespace RecipeGenerator.DTOs
{
    public class TrendingRecipeDto
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CookTime { get; set; }
        public string Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Img { get; set; }
        public int FetchCount { get; set; }
        public List<string> Ingredients { get; set; }
        public List<string> DietaryRestrictions { get; set; }
        public bool IsFavourite { get; set; }
    }
}