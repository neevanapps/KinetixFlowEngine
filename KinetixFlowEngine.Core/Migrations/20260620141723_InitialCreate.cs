using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    EngineVersion = table.Column<string>(type: "text", nullable: false),
                    SnapshotTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    VWAP = table.Column<decimal>(type: "numeric", nullable: false),
                    ATR15m = table.Column<double>(type: "double precision", nullable: false),
                    FundingRate = table.Column<double>(type: "double precision", nullable: false),
                    FundingPressure = table.Column<double>(type: "double precision", nullable: false),
                    OIChange = table.Column<double>(type: "double precision", nullable: false),
                    ScoreZ10m = table.Column<double>(type: "double precision", nullable: false),
                    ScoreZ30m = table.Column<double>(type: "double precision", nullable: false),
                    ScoreZ60m = table.Column<double>(type: "double precision", nullable: false),
                    VelocityZ10m = table.Column<double>(type: "double precision", nullable: false),
                    VelocityZ30m = table.Column<double>(type: "double precision", nullable: false),
                    VelocityZ60m = table.Column<double>(type: "double precision", nullable: false),
                    ImbalanceZ10m = table.Column<double>(type: "double precision", nullable: false),
                    ImbalanceZ30m = table.Column<double>(type: "double precision", nullable: false),
                    ImbalanceZ60m = table.Column<double>(type: "double precision", nullable: false),
                    CompressionZ10m = table.Column<double>(type: "double precision", nullable: false),
                    CompressionZ30m = table.Column<double>(type: "double precision", nullable: false),
                    CompressionZ60m = table.Column<double>(type: "double precision", nullable: false),
                    ExhaustionZ10m = table.Column<double>(type: "double precision", nullable: false),
                    ExhaustionZ30m = table.Column<double>(type: "double precision", nullable: false),
                    ExhaustionZ60m = table.Column<double>(type: "double precision", nullable: false),
                    Momentum10m = table.Column<double>(type: "double precision", nullable: false),
                    Momentum30m = table.Column<double>(type: "double precision", nullable: false),
                    Momentum60m = table.Column<double>(type: "double precision", nullable: false),
                    Acceleration10m = table.Column<double>(type: "double precision", nullable: false),
                    Acceleration30m = table.Column<double>(type: "double precision", nullable: false),
                    Acceleration60m = table.Column<double>(type: "double precision", nullable: false),
                    Persistence10m = table.Column<double>(type: "double precision", nullable: false),
                    Persistence30m = table.Column<double>(type: "double precision", nullable: false),
                    Persistence60m = table.Column<double>(type: "double precision", nullable: false),
                    NetPressure10m = table.Column<double>(type: "double precision", nullable: false),
                    NetPressure30m = table.Column<double>(type: "double precision", nullable: false),
                    NetPressure60m = table.Column<double>(type: "double precision", nullable: false),
                    FlowImpact10m = table.Column<double>(type: "double precision", nullable: false),
                    FlowImpact30m = table.Column<double>(type: "double precision", nullable: false),
                    FlowImpact60m = table.Column<double>(type: "double precision", nullable: false),
                    ER5_10m = table.Column<double>(type: "double precision", nullable: false),
                    ER5_30m = table.Column<double>(type: "double precision", nullable: false),
                    ER5_60m = table.Column<double>(type: "double precision", nullable: false),
                    ER30_10m = table.Column<double>(type: "double precision", nullable: false),
                    ER30_30m = table.Column<double>(type: "double precision", nullable: false),
                    ER30_60m = table.Column<double>(type: "double precision", nullable: false),
                    DepthImbalance10m = table.Column<double>(type: "double precision", nullable: false),
                    DepthImbalance30m = table.Column<double>(type: "double precision", nullable: false),
                    DepthImbalance60m = table.Column<double>(type: "double precision", nullable: false),
                    DepthBullPct10m = table.Column<double>(type: "double precision", nullable: false),
                    DepthBullPct30m = table.Column<double>(type: "double precision", nullable: false),
                    DepthBullPct60m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallAge10m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallAge30m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallAge60m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallAge10m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallAge30m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallAge60m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallQty10m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallQty30m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallQty60m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallQty10m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallQty30m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallQty60m = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketOutcomes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ATRAtReview = table.Column<decimal>(type: "numeric", nullable: false),
                    Price15m = table.Column<decimal>(type: "numeric", nullable: false),
                    Price30m = table.Column<decimal>(type: "numeric", nullable: false),
                    Price60m = table.Column<decimal>(type: "numeric", nullable: false),
                    Move15m = table.Column<decimal>(type: "numeric", nullable: false),
                    Move30m = table.Column<decimal>(type: "numeric", nullable: false),
                    Move60m = table.Column<decimal>(type: "numeric", nullable: false),
                    MovePct15m = table.Column<double>(type: "double precision", nullable: false),
                    MovePct30m = table.Column<double>(type: "double precision", nullable: false),
                    MovePct60m = table.Column<double>(type: "double precision", nullable: false),
                    MoveATR15m = table.Column<double>(type: "double precision", nullable: false),
                    MoveATR30m = table.Column<double>(type: "double precision", nullable: false),
                    MoveATR60m = table.Column<double>(type: "double precision", nullable: false),
                    Direction15m = table.Column<string>(type: "text", nullable: false),
                    Direction30m = table.Column<string>(type: "text", nullable: false),
                    Direction60m = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketOutcomes_MarketSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "MarketSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelReviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DirectionalBias = table.Column<string>(type: "text", nullable: false),
                    LongConfidence = table.Column<int>(type: "integer", nullable: false),
                    ShortConfidence = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TrendQuality = table.Column<int>(type: "integer", nullable: false),
                    FlowQuality = table.Column<int>(type: "integer", nullable: false),
                    RegimeQuality = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "text", nullable: false),
                    DominantIntent = table.Column<string>(type: "text", nullable: false),
                    MarketStructure = table.Column<string>(type: "text", nullable: false),
                    StateAssessment = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    BehaviorEvidenceJson = table.Column<string>(type: "text", nullable: false),
                    KeyDriversJson = table.Column<string>(type: "text", nullable: false),
                    ContradictionsJson = table.Column<string>(type: "text", nullable: false),
                    RawResponseJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelReviews_MarketSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "MarketSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketOutcomes_SnapshotId",
                table: "MarketOutcomes",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelReviews_SnapshotId",
                table: "ModelReviews",
                column: "SnapshotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketOutcomes");

            migrationBuilder.DropTable(
                name: "ModelReviews");

            migrationBuilder.DropTable(
                name: "MarketSnapshots");
        }
    }
}
