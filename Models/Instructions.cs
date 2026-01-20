namespace RecipeGenerator.Models
{
    public class Instructions
    {
        public int Id { get; set; }
        public string Instruction { get; set; }
        public Recipe Recipe { get; set; }
    }
}
