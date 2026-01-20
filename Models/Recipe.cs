using RecipeGenerator.Models;
using System;

public class Recipe
{
	public int RecipeId { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public int CookTime { get; set; }
	
	// One-to-one relationship with Instructions
	public Instructions Instructions { get; set; }
	public int InstructionsId { get; set; }
	
	public string Difficulty { get; set; }
	public ICollection <RecipeDietaryRestrictions> RecipeDietaryRestrictions { get; set; }
    public ICollection <UserSavedRecipes> UserSavedRecipes { get; set; }
    public ICollection <RecipeIngredients>	RecipeIngredients { get; set; }
	public DateTime CreatedAt { get; set; }
}
