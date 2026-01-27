namespace RecipeGenerator.DTOs
{
    public class RecipeMatchRequestDto
    {
        public List<int> IngredientIds { get; set; } = new();
        public List<int>? DietaryRestrictionIds { get; set; }
    }
}
