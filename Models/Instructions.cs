namespace RecipeGenerator.Models
{
    public class Instructions
    {
        public int InstructionsId { get; set; }  // Primary Key
        public string Instruction { get; set; }
        
        // Foreign key to Recipe
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
