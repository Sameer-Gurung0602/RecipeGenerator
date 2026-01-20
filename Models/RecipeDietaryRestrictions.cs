namespace RecipeGenerator.Models
{
    public class RecipeDietaryRestrictions
    {
        public int RecipeID { get; set; }
        public Recipe Recipe { get; set; }
        public int DietaryRestrictionID { get; set; }
        public DietaryRestrictions DietaryRestrictions { get; set; }
    }
}
