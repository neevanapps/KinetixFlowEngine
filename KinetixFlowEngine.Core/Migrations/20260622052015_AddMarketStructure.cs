using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom15mHigh",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom15mLow",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom180mHigh",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom180mLow",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom45mHigh",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom45mLow",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Trend15m",
                table: "MarketSnapshots",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Trend180m",
                table: "MarketSnapshots",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Trend45m",
                table: "MarketSnapshots",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceFrom15mHigh",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom15mLow",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom180mHigh",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom180mLow",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom45mHigh",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom45mLow",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Trend15m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Trend180m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Trend45m",
                table: "MarketSnapshots");
        }
    }
}
