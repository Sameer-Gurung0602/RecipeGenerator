using System;

namespace RecipeGenerator.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CookTime { get; set; }
        public string Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Img { get; set; }
        public int FetchCount { get; set; }
        
        public int InstructionsId { get; set; }
        public Instructions Instructions { get; set; }
        
        public ICollection<DietaryRestrictions> DietaryRestrictions { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Ingredient> Ingredients { get; set; }
    }
}
