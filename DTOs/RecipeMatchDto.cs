namespace RecipeGenerator.DTOs
{
    public class RecipeMatchDto
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CookTime { get; set; }
        public string Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Img { get; set; }  // Added
        public int MatchPercentage { get; set; }
        public int TotalIngredientsRequired { get; set; }
        public int IngredientsMatched { get; set; }
        public bool IsFavourite { get; set; }
        public List<string> MatchedIngredients { get; set; } = new();
        public List<string> MissingIngredients { get; set; } = new();
        public List<string> DietaryRestrictions { get; set; } = new();  // ✅ ADDED
    }
}
