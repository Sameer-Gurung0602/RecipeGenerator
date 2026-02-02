namespace RecipeGenerator.DTOs
{
    public class RecipeMatchRequestDto
    {
        public List<int>? IngredientIds { get; set; }  // ✅ Make nullable
        public List<int>? DietaryRestrictionIds { get; set; }
    }
}
