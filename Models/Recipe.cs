using System;

public class Recipe
{
	public int RecipeId { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public int CookTime { get; set; }
	public Instruction Instructions { get; set; }
	public int InstructorId { get; set; }
	public string Difficulty { get; set; }
	public ICollection <RecipeDietaryRestrictions> RecipeDietaryRestrictions { get; set; }
    public ICollection <UserSavedRecipe> UserSavedRecipes { get; set; }
    public ICollection <RecipeIngredients>	RecipeIngredients { get; set; }
	public DateTime CreatedAt { get; set; }
}
