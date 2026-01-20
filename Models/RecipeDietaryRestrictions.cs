namespace RecipeGenerator.Models
{
    public class RecipeDietaryRestrictions
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public int DietaryRestrictionsId { get; set; }
        public DietaryRestrictions DietaryRestrictions { get; set; }
    }
}
