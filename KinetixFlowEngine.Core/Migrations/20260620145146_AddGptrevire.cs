using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGptrevire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawResponse",
                table: "ModelReviews",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "ModelReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tradeability",
                table: "ModelReviews",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawResponse",
                table: "ModelReviews");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "ModelReviews");

            migrationBuilder.DropColumn(
                name: "Tradeability",
                table: "ModelReviews");
        }
    }
}
