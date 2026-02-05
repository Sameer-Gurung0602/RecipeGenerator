using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.DTOs;

namespace RecipeGenerator.Services
{
    public class RecipeService
    {
        private readonly RecipeGeneratorDbContext _context;

        public RecipeService(RecipeGeneratorDbContext context)
        {
            _context = context;
        }

        // ✅ ADD THIS HELPER METHOD
        private static int GetDifficultyOrder(string difficulty)
        {
            return difficulty?.ToLower() switch
            {
                "easy" => 1,
                "medium" => 2,
                "hard" => 3,
                _ => 4 // Unknown difficulties go last
            };
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

            // ✅ ADD THIS - Apply sorting in-memory with custom difficulty logic
            var sortedRecipes = sortBy?.ToLower() switch
            {
                "date" => sortOrder?.ToLower() == "desc"
                    ? recipes.OrderByDescending(r => r.CreatedAt)
                    : recipes.OrderBy(r => r.CreatedAt),
                "cooktime" => sortOrder?.ToLower() == "desc"
                    ? recipes.OrderByDescending(r => r.CookTime)
                    : recipes.OrderBy(r => r.CookTime),
                "difficulty" => sortOrder?.ToLower() == "desc"
                    ? recipes.OrderByDescending(r => GetDifficultyOrder(r.Difficulty))
                    : recipes.OrderBy(r => GetDifficultyOrder(r.Difficulty)),
                _ => recipes.OrderBy(r => r.RecipeId) // Default sort by ID
            };

            return sortedRecipes.ToList();
        }

        // method to query database for a single recipe by ID
        public async Task<RecipeDto?> GetRecipeById(int id, int userId)
        {
            // Increment fetch count first
            await IncrementFetchCount(id);

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
            List<int>? ingredientIds = null,
            List<int>? dietaryRestrictionIds = null,
            string? sortBy = null,
            string? sortOrder = "asc")
        {
            var query = _context.Recipes
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .Include(r => r.Users)
                .AsQueryable();

            if (dietaryRestrictionIds != null && dietaryRestrictionIds.Any())
            {
                query = query.Where(r =>
                    dietaryRestrictionIds.All(drId =>
                        r.DietaryRestrictions.Any(dr => dr.DietaryRestrictionsId == drId)
                    )
                );
            }

            var allRecipes = await query.ToListAsync();

            if (ingredientIds != null && ingredientIds.Any())
            {
                allRecipes = allRecipes
                    .Where(r => r.Ingredients.Any(i => ingredientIds.Contains(i.IngredientId)))
                    .ToList();
            }

            var matchedRecipes = allRecipes.Select(recipe =>
            {
                var recipeIngredientIds = recipe.Ingredients.Select(i => i.IngredientId).ToList();
                var totalIngredientsRequired = recipeIngredientIds.Count;

                int ingredientsMatched = 0;
                int matchPercentage = 0;
                List<int> matchedIngredientIds = new();
                List<int> missingIngredientIds = new();

                if (ingredientIds != null && ingredientIds.Any())
                {
                    matchedIngredientIds = recipeIngredientIds.Intersect(ingredientIds).ToList();
                    missingIngredientIds = recipeIngredientIds.Except(ingredientIds).ToList();
                    ingredientsMatched = matchedIngredientIds.Count;
                    matchPercentage = totalIngredientsRequired > 0
                        ? (int)Math.Round((double)ingredientsMatched / totalIngredientsRequired * 100)
                        : 0;
                }
                else
                {
                    missingIngredientIds = recipeIngredientIds;
                }

                return new RecipeMatchDto
                {
                    RecipeId = recipe.RecipeId,
                    Name = recipe.Name,
                    Description = recipe.Description,
                    CookTime = recipe.CookTime,
                    Difficulty = recipe.Difficulty,
                    CreatedAt = recipe.CreatedAt,
                    Img = recipe.Img,
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
                        .ToList(),
                    DietaryRestrictions = recipe.DietaryRestrictions.Select(d => d.Name).ToList()
                };
            })
            .ToList();

            // ✅ UPDATE THIS - Use custom difficulty ordering
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
                    ? matchedRecipes.OrderByDescending(r => GetDifficultyOrder(r.Difficulty)).ToList()
                    : matchedRecipes.OrderBy(r => GetDifficultyOrder(r.Difficulty)).ToList(),
                _ => matchedRecipes.OrderByDescending(r => r.MatchPercentage).ToList()
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


        public async Task IncrementFetchCount(int recipeId) //increase recipefetchcount for every request
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe != null)
            {
                recipe.FetchCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TrendingRecipeDto>> GetTrendingRecipes(int count = 4) //order by recipefetchcount
        {
            var trendingRecipes = await _context.Recipes
                .Include(r => r.Ingredients)
                .Include(r => r.DietaryRestrictions)
                .OrderByDescending(r => r.FetchCount)
                .Take(count)
                .Select(r => new TrendingRecipeDto
                {
                    RecipeId = r.RecipeId,
                    Name = r.Name,
                    Description = r.Description,
                    CookTime = r.CookTime,
                    Difficulty = r.Difficulty,
                    CreatedAt = r.CreatedAt,
                    Img = r.Img,
                    FetchCount = r.FetchCount,
                    Ingredients = r.Ingredients.Select(i => i.IngredientName).ToList(),
                    DietaryRestrictions = r.DietaryRestrictions.Select(d => d.Name).ToList()
                })
                .ToListAsync();

            return trendingRecipes;
        }
    }
}