using System;

public class Ingredient
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; }
    public ICollection <RecipeIngredients> RecipeIngredients { get; set; }
}
