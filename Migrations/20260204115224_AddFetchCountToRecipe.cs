using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddFetchCountToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FetchCount",
                table: "Recipes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FetchCount",
                table: "Recipes");
        }
    }
}
