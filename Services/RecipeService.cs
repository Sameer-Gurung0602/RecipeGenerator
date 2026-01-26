using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.DTOs;


namespace RecipeGenerator.Services
{
    public class RecipeService
    {
        // initialise database connection
        private readonly RecipeGeneratorDbContext _context;

        // constructor - dependency injection
        public RecipeService(RecipeGeneratorDbContext context)
        {
            _context = context;
        }

        // method to query database for all recipes
        public async Task<IEnumerable<RecipeDto>> GetAllRecipes()
        {
            var recipes = await _context.Recipes
                .Include(r => r.Instructions)
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .Select(r => new RecipeDto      // transform each recipe into recipe DTO
                {
                    RecipeId = r.RecipeId,
                    Name = r.Name,
                    Description = r.Description,
                    CookTime = r.CookTime,
                    Difficulty = r.Difficulty,
                    CreatedAt = r.CreatedAt,
                    Instructions = new InstructionsDto
                    {
                        InstructionsId = r.Instructions.InstructionsId,
                        Instruction = r.Instructions.Instruction
                    },
                    DietaryRestrictions = r.DietaryRestrictions.Select(d => d.Name).ToList(),
                    Ingredients = r.Ingredients.Select(i => i.IngredientName).ToList()
                })
                .ToListAsync();

            return recipes;
        }

        // method to query database for a single recipe by ID
        public async Task<RecipeDto?> GetRecipeById(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Instructions)
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .Where(r => r.RecipeId == id)
                .Select(r => new RecipeDto
                {
                    RecipeId = r.RecipeId,
                    Name = r.Name,
                    Description = r.Description,
                    CookTime = r.CookTime,
                    Difficulty = r.Difficulty,
                    CreatedAt = r.CreatedAt,
                    Instructions = r.Instructions != null ? new InstructionsDto
                    {
                        InstructionsId = r.Instructions.InstructionsId,
                        Instruction = r.Instructions.Instruction
                    } : null,
                    DietaryRestrictions = r.DietaryRestrictions.Select(d => d.Name).ToList(),
                    Ingredients = r.Ingredients.Select(i => i.IngredientName).ToList()
                })
                .FirstOrDefaultAsync();

            return recipe;
        }

        public async Task<DietaryRestrictionsDto?> GetRecipeDietaryRestrictions(int id)
        {
            var dietaryRestrictions = await _context.Recipes
                .Where(r => r.RecipeId == id)
                .Include(r => r.DietaryRestrictions)
                .Select(r => new DietaryRestrictionsDto
                {
                    DietaryRestrictions = r.DietaryRestrictions.Select(d => d.Name).ToList()
                })
                .FirstOrDefaultAsync();

            return dietaryRestrictions;
            }
    }
}