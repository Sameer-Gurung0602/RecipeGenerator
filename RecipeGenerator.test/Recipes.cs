using RecipeGenerator.Data;
using RecipeGenerator.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeGenerator.test
{
    [Collection("Database collection")]
    public class Recipes : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public Recipes(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Database_Should_Be_Created()
        {

        }
    }
}


