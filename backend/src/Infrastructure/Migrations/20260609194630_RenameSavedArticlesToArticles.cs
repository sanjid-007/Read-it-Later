using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadItLater.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameSavedArticlesToArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedArticles",
                table: "SavedArticles");

            migrationBuilder.RenameTable(
                name: "SavedArticles",
                newName: "Articles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Articles",
                table: "Articles",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Articles",
                table: "Articles");

            migrationBuilder.RenameTable(
                name: "Articles",
                newName: "SavedArticles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedArticles",
                table: "SavedArticles",
                column: "Id");
        }
    }
}
