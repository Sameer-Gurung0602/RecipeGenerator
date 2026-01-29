using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RecipeGenerator.Data;
using RecipeGenerator.DTOs;
using RecipeGenerator.Models;
using System.Net;

namespace RecipeGenerator.Services
{
    public class FavouritesService
    {
        // initialise database connection
        private readonly RecipeGeneratorDbContext _context;

        // constructor - dependency injection
        public FavouritesService(RecipeGeneratorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecipeDto>> GetAllFavourites(int id)
        {
            var recipes = await _context.Recipes
                .Include(r => r.Ingredients)
                .Include(r => r.Instructions)
                .Include(r => r.DietaryRestrictions)
                .Include(r => r.Users)
                .Where(r => r.Users.Any(u => u.UserId == id))
                .ToListAsync();

            return recipes.Select(r => new RecipeDto
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
                Ingredients = r.Ingredients.Select(i => i.IngredientName).ToList()
            });
        }

        public async Task<bool> RemoveFavourite(int recipeId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);

            if (recipe == null)
            {
                return false;
            }

            var user = recipe.Users.FirstOrDefault(u => u.UserId == 9);
            if (user == null)
            {
                return false;
            }

            recipe.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}