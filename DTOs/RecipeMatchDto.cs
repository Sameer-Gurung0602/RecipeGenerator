namespace RecipeGenerator.DTOs
{
    public class RecipeMatchDto
    {
        public int RecipeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CookTime { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int MatchPercentage { get; set; }
        public int TotalIngredientsRequired { get; set; }
        public int IngredientsMatched { get; set; }
        public List <string> MatchedIngredients { get; set; } = new();
        public List<string> MissingIngredients { get; set; } = new();

    }
}
