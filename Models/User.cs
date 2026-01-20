using RecipeGenerator.Models;
using System;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public ICollection<UserSavedRecipes> UserSavedRecipes { get; set; }
}
