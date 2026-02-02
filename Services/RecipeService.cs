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
        public async Task<IEnumerable<RecipeDto>> GetAllRecipes(int userId, string? sortBy = null, string? sortOrder = "asc")
        {
            var query = _context.Recipes
                .Include(r => r.Instructions)
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .Include(r => r.Users)
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
                    Img = r.Img,  // Added
                    Instructions = new InstructionsDto
                    {
                        InstructionsId = r.Instructions.InstructionsId,
                        Instruction = r.Instructions.Instruction
                    },
                    DietaryRestrictions = r.DietaryRestrictions.Select(d => d.Name).ToList(),
                    Ingredients = r.Ingredients.Select(i => i.IngredientName).ToList(),
                    IsFavourite = r.Users.Any(u => u.UserId == userId)
                })
                .ToListAsync();

            return recipes;
        }

        // method to query database for a single recipe by ID
        public async Task<RecipeDto?> GetRecipeById(int id, int userId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Instructions)
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .Include(r => r.Users)
                .Where(r => r.RecipeId == id)
                .Select(r => new RecipeDto
                {
                    RecipeId = r.RecipeId,
                    Name = r.Name,
                    Description = r.Description,
                    CookTime = r.CookTime,
                    Difficulty = r.Difficulty,
                    CreatedAt = r.CreatedAt,
                    Img = r.Img,  // Added
                    Instructions = r.Instructions != null ? new InstructionsDto
                    {
                        InstructionsId = r.Instructions.InstructionsId,
                        Instruction = r.Instructions.Instruction
                    } : null,
                    DietaryRestrictions = r.DietaryRestrictions.Select(d => d.Name).ToList(),
                    Ingredients = r.Ingredients.Select(i => i.IngredientName).ToList(),
                    IsFavourite = r.Users.Any(u => u.UserId == userId)
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

        public async Task<DietaryRestrictionsDto> GetDietaryRestrictions()
        {
            var dietaryRestrictions = await _context.DietaryRestrictions
                .Select(d => d.Name)
                .ToListAsync();

            return new DietaryRestrictionsDto
            {
                DietaryRestrictions = dietaryRestrictions
            };

        }

        public async Task<IEnumerable<RecipeMatchDto>> GetMatchingRecipes(
            int userId,
            List<int> ingredientIds,
            List<int>? dietaryRestrictionIds = null,
            string? sortBy = null,
            string? sortOrder = "asc")
        {
            // Start with base query - get all recipes with their related data
            var query = _context.Recipes
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .Include(r => r.Users)
                .AsQueryable();

            // Filter by dietary restrictions if provided
            if (dietaryRestrictionIds != null && dietaryRestrictionIds.Any())
            {
                query = query.Where(r =>
                    dietaryRestrictionIds.All(drId =>
                        r.DietaryRestrictions.Any(dr => dr.DietaryRestrictionsId == drId)
                    )
                );
            }

            // Execute query and perform matching in memory for complex calculations
            var allRecipes = await query.ToListAsync();

            // Calculate match percentage for each recipe
            var matchedRecipes = allRecipes.Select(recipe =>
            {
                var recipeIngredientIds = recipe.Ingredients.Select(i => i.IngredientId).ToList();
                var totalIngredientsRequired = recipeIngredientIds.Count;

                // Find matched and missing ingredients
                var matchedIngredientIds = recipeIngredientIds.Intersect(ingredientIds).ToList();
                var missingIngredientIds = recipeIngredientIds.Except(ingredientIds).ToList();

                var ingredientsMatched = matchedIngredientIds.Count;
                var matchPercentage = totalIngredientsRequired > 0
                    ? (int)Math.Round((double)ingredientsMatched / totalIngredientsRequired * 100)
                    : 0;

                return new RecipeMatchDto
                {
                    RecipeId = recipe.RecipeId,
                    Name = recipe.Name,
                    Description = recipe.Description,
                    CookTime = recipe.CookTime,
                    Difficulty = recipe.Difficulty,
                    CreatedAt = recipe.CreatedAt,
                    Img = recipe.Img,  // Added
                    MatchPercentage = matchPercentage,
                    TotalIngredientsRequired = totalIngredientsRequired,
                    IngredientsMatched = ingredientsMatched,
                    IsFavourite = recipe.Users.Any(u => u.UserId == userId),
                    MatchedIngredients = recipe.Ingredients
                        .Where(i => matchedIngredientIds.Contains(i.IngredientId))
                        .Select(i => i.IngredientName)
                        .ToList(),
                    MissingIngredients = recipe.Ingredients
                        .Where(i => missingIngredientIds.Contains(i.IngredientId))
                        .Select(i => i.IngredientName)
                        .ToList()
                };
            })
            .ToList();

            // Apply sorting
            matchedRecipes = sortBy?.ToLower() switch
            {
                "match" or "matchpercentage" => sortOrder?.ToLower() == "desc"
                    ? matchedRecipes.OrderByDescending(r => r.MatchPercentage).ToList()
                    : matchedRecipes.OrderBy(r => r.MatchPercentage).ToList(),
                "date" => sortOrder?.ToLower() == "desc"
                    ? matchedRecipes.OrderByDescending(r => r.CreatedAt).ToList()
                    : matchedRecipes.OrderBy(r => r.CreatedAt).ToList(),
                "cooktime" => sortOrder?.ToLower() == "desc"
                    ? matchedRecipes.OrderByDescending(r => r.CookTime).ToList()
                    : matchedRecipes.OrderBy(r => r.CookTime).ToList(),
                "difficulty" => sortOrder?.ToLower() == "desc"
                    ? matchedRecipes.OrderByDescending(r => r.Difficulty).ToList()
                    : matchedRecipes.OrderBy(r => r.Difficulty).ToList(),
                _ => matchedRecipes.OrderByDescending(r => r.MatchPercentage).ToList() // Default: best matches first
            };

            return matchedRecipes;
        }


        public async Task<IEnumerable<IngredientDto>> GetAllIngredients()
        {
            var ingredients = await _context.Ingredients
                .Select(i => new IngredientDto
                {
                    IngredientId = i.IngredientId,
                    IngredientName = i.IngredientName
                })
                .OrderBy(i => i.IngredientName)
                .ToListAsync();
            return ingredients;
        }
    }
}