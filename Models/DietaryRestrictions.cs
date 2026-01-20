namespace RecipeGenerator.Models
{
    public class DietaryRestrictions
    {
        public int DietaryRestrictionsId { get; set; }
        public string Name { get; set; }
        public ICollection<RecipeDietaryRestrictions> RecipeDietaryRestrictions { get; set; }

    }
}
