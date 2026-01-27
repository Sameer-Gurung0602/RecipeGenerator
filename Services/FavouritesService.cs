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
               Instructions = new InstructionsDto
               {
                   InstructionsId = r.Instructions.InstructionsId,
                   Instruction = r.Instructions.Instruction
               },
               DietaryRestrictions = r.DietaryRestrictions.Select(d => d.Name).ToList(),
               Ingredients = r.Ingredients.Select(i => i.IngredientName).ToList()
           });
       }
    }
}