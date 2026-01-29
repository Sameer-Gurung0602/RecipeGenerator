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

        //public async Task<ServiceResult> AddFavourite(int id)



        public async Task<ServiceResult> RemoveFavourite(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .Include(r => r.Instructions)
                .Include(r => r.DietaryRestrictions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RecipeId == id);

            if (recipe == null)
            {
                return ServiceResult.NotFound($"Recipe with ID {id} not found.");
            }

            var user = recipe.Users.FirstOrDefault(u => u.UserId == 9);
            if (user == null)
            {
                return ServiceResult.NotFound($"Recipe with ID {id} is not in favourites.");
            }

            recipe.Users.Remove(user);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }
    }

    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }

        public static ServiceResult Success() => new ServiceResult { IsSuccess = true };

        public static ServiceResult NotFound(string message) => new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = "NotFound"
        };

        public static ServiceResult Conflict(string message) => new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorType = "Conflict"
        };
    }
}