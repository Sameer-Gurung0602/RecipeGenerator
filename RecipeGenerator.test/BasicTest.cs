using RecipeGenerator.Data;
using Microsoft.Extensions.DependencyInjection;

namespace RecipeGenerator.Test
{
    [Collection("Database collection")]
    public class BasicTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public BasicTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Database_Should_Be_Created()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RecipeGeneratorDbContext>();

            // Act
            var canConnect = db.Database.CanConnect();
            var userCount = db.Users.Count();

            // Assert
            Assert.True(canConnect, "Should be able to connect to database");
            Assert.True(userCount > 0, $"Should have users in database. Found: {userCount}");
        }
    }
}