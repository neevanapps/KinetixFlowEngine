using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommandedAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecommendedAction",
                table: "ModelReviews",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecommendedAction",
                table: "ModelReviews");
        }
    }
}
