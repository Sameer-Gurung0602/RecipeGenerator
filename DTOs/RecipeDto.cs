namespace RecipeGenerator.DTOs
{
    public class RecipeDto
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CookTime { get; set; }
        public string Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Img { get; set; }  // Added
        public InstructionsDto? Instructions { get; set; }
        public List<string> DietaryRestrictions { get; set; } = new();
        public List<string> Ingredients { get; set; } = new();
    }
}
