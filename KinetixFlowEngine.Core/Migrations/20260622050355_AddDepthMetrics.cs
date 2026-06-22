using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDepthMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AskConsumption10m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AskConsumption30m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AskConsumption60m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BidConsumption10m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BidConsumption30m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BidConsumption60m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BodyPct10m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BodyPct30m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BodyPct60m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BullishPersistence10m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BullishPersistence30m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BullishPersistence60m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom10mHigh",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom10mLow",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom30mHigh",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom30mLow",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom60mHigh",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFrom60mLow",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LowerWickPct10m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LowerWickPct30m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LowerWickPct60m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Trend10m",
                table: "MarketSnapshots",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Trend30m",
                table: "MarketSnapshots",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Trend600m",
                table: "MarketSnapshots",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "UpperWickPct10m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UpperWickPct30m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UpperWickPct60m",
                table: "MarketSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AskConsumption10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "AskConsumption30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "AskConsumption60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BidConsumption10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BidConsumption30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BidConsumption60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BodyPct10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BodyPct30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BodyPct60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BullishPersistence10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BullishPersistence30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "BullishPersistence60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom10mHigh",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom10mLow",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom30mHigh",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom30mLow",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom60mHigh",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "DistanceFrom60mLow",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "LowerWickPct10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "LowerWickPct30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "LowerWickPct60m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Trend10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Trend30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "Trend600m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "UpperWickPct10m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "UpperWickPct30m",
                table: "MarketSnapshots");

            migrationBuilder.DropColumn(
                name: "UpperWickPct60m",
                table: "MarketSnapshots");
        }
    }
}
