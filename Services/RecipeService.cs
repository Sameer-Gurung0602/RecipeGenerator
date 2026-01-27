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
        public async Task<IEnumerable<RecipeDto>> GetAllRecipes(string? sortBy = null, string? sortOrder = "asc")
        {
            var query = _context.Recipes
                .Include(r => r.Instructions)
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .AsQueryable();

            // Apply sorting        
            query = sortBy?.ToLower() switch
            {
                "date" => sortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                "cooktime" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.CookTime)
                    : query.OrderBy(r => r.CookTime),
                "difficulty" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.Difficulty)
                    : query.OrderBy(r => r.Difficulty),
                _ => query.OrderBy(r => r.RecipeId) // Default sort by ID
            };

            var recipes = await query
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
    }
}