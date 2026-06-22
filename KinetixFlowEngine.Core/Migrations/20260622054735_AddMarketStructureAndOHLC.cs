using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketStructureAndOHLC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Close10m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Close30m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Close60m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFromVWAP",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFromVWAPPct",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "High10m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "High30m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "High60m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Low10m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Low30m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Low60m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Open10m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Open30m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Open60m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RangeHigh10m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RangeHigh30m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RangeHigh60m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RangeLow10m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RangeLow30m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RangeLow60m",
                table: "MarketSnapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Close10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Close30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Close60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFromVWAP",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFromVWAPPct",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "High10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "High30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "High60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Low10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Low30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Low60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Open10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Open30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Open60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "RangeHigh10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "RangeHigh30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "RangeHigh60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "RangeLow10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "RangeLow30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "RangeLow60m",
                table: "MarketSnapshots");
        }
    }
}
