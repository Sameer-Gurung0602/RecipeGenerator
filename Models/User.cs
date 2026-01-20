using System;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public ICollection <UserSavedRecipe> UserSavedRecipes { get; set; }
}
