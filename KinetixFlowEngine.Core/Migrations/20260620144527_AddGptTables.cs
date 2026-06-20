using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGptTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Correct15m",
                table: "ModelReviews",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Correct30m",
                table: "ModelReviews",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Correct60m",
                table: "ModelReviews",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FutureMove15m",
                table: "ModelReviews",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FutureMove30m",
                table: "ModelReviews",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FutureMove60m",
                table: "ModelReviews",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Correct15m",
                table: "ModelReviews");

            migrationBuilder.DropColumn(
                name: "Correct30m",
                table: "ModelReviews");

            migrationBuilder.DropColumn(
                name: "Correct60m",
                table: "ModelReviews");

            migrationBuilder.DropColumn(
                name: "FutureMove15m",
                table: "ModelReviews");

            migrationBuilder.DropColumn(
                name: "FutureMove30m",
                table: "ModelReviews");

            migrationBuilder.DropColumn(
                name: "FutureMove60m",
                table: "ModelReviews");
        }
    }
}
