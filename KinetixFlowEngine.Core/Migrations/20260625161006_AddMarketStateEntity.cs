using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KinetixFlowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketStateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketOutcomes");

            migrationBuilder.DropTable(
                name: "MarketPrices");

            migrationBuilder.DropTable(
                name: "ModelReviews");

            migrationBuilder.DropTable(
                name: "MarketSnapshots");

            migrationBuilder.CreateTable(
                name: "market_state",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    Timeframe = table.Column<short>(type: "smallint", nullable: false),
                    EngineBuild = table.Column<int>(type: "integer", nullable: false),
                    QualityScore = table.Column<byte>(type: "smallint", nullable: false),
                    Regime = table.Column<short>(type: "smallint", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_state", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_market_state_Sequence",
                table: "market_state",
                column: "Sequence");

            migrationBuilder.CreateIndex(
                name: "IX_market_state_Timeframe_TimestampUtc",
                table: "market_state",
                columns: new[] { "Timeframe", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_market_state_TimestampUtc",
                table: "market_state",
                column: "TimestampUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "market_state");

            migrationBuilder.CreateTable(
                name: "MarketPrices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Price = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ATR15m = table.Column<double>(type: "double precision", nullable: false),
                    Acceleration10m = table.Column<double>(type: "double precision", nullable: false),
                    Acceleration30m = table.Column<double>(type: "double precision", nullable: false),
                    Acceleration60m = table.Column<double>(type: "double precision", nullable: false),
                    AskConsumption10m = table.Column<double>(type: "double precision", nullable: false),
                    AskConsumption30m = table.Column<double>(type: "double precision", nullable: false),
                    AskConsumption60m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallAge10m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallAge30m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallAge60m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallQty10m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallQty30m = table.Column<double>(type: "double precision", nullable: false),
                    AskWallQty60m = table.Column<double>(type: "double precision", nullable: false),
                    BidConsumption10m = table.Column<double>(type: "double precision", nullable: false),
                    BidConsumption30m = table.Column<double>(type: "double precision", nullable: false),
                    BidConsumption60m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallAge10m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallAge30m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallAge60m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallQty10m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallQty30m = table.Column<double>(type: "double precision", nullable: false),
                    BidWallQty60m = table.Column<double>(type: "double precision", nullable: false),
                    BodyPct10m = table.Column<double>(type: "double precision", nullable: false),
                    BodyPct30m = table.Column<double>(type: "double precision", nullable: false),
                    BodyPct60m = table.Column<double>(type: "double precision", nullable: false),
                    BullishPersistence10m = table.Column<double>(type: "double precision", nullable: false),
                    BullishPersistence30m = table.Column<double>(type: "double precision", nullable: false),
                    BullishPersistence60m = table.Column<double>(type: "double precision", nullable: false),
                    Close10m = table.Column<decimal>(type: "numeric", nullable: false),
                    Close30m = table.Column<decimal>(type: "numeric", nullable: false),
                    Close60m = table.Column<decimal>(type: "numeric", nullable: false),
                    CompressionZ10m = table.Column<double>(type: "double precision", nullable: false),
                    CompressionZ30m = table.Column<double>(type: "double precision", nullable: false),
                    CompressionZ60m = table.Column<double>(type: "double precision", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DepthBullPct10m = table.Column<double>(type: "double precision", nullable: false),
                    DepthBullPct30m = table.Column<double>(type: "double precision", nullable: false),
                    DepthBullPct60m = table.Column<double>(type: "double precision", nullable: false),
                    DepthImbalance10m = table.Column<double>(type: "double precision", nullable: false),
                    DepthImbalance30m = table.Column<double>(type: "double precision", nullable: false),
                    DepthImbalance60m = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFrom10mHigh = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFrom10mLow = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFrom30mHigh = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFrom30mLow = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFrom60mHigh = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFrom60mLow = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFromVWAP = table.Column<double>(type: "double precision", nullable: false),
                    DistanceFromVWAPPct = table.Column<double>(type: "double precision", nullable: false),
                    ER30_10m = table.Column<double>(type: "double precision", nullable: false),
                    ER30_30m = table.Column<double>(type: "double precision", nullable: false),
                    ER30_60m = table.Column<double>(type: "double precision", nullable: false),
                    ER5_10m = table.Column<double>(type: "double precision", nullable: false),
                    ER5_30m = table.Column<double>(type: "double precision", nullable: false),
                    ER5_60m = table.Column<double>(type: "double precision", nullable: false),
                    EngineVersion = table.Column<string>(type: "text", nullable: false),
                    ExhaustionZ10m = table.Column<double>(type: "double precision", nullable: false),
                    ExhaustionZ30m = table.Column<double>(type: "double precision", nullable: false),
                    ExhaustionZ60m = table.Column<double>(type: "double precision", nullable: false),
                    FlowImpact10m = table.Column<double>(type: "double precision", nullable: false),
                    FlowImpact30m = table.Column<double>(type: "double precision", nullable: false),
                    FlowImpact60m = table.Column<double>(type: "double precision", nullable: false),
                    FundingPressure = table.Column<double>(type: "double precision", nullable: false),
                    FundingRate = table.Column<double>(type: "double precision", nullable: false),
                    High10m = table.Column<decimal>(type: "numeric", nullable: false),
                    High30m = table.Column<decimal>(type: "numeric", nullable: false),
                    High60m = table.Column<decimal>(type: "numeric", nullable: false),
                    ImbalanceZ10m = table.Column<double>(type: "double precision", nullable: false),
                    ImbalanceZ30m = table.Column<double>(type: "double precision", nullable: false),
                    ImbalanceZ60m = table.Column<double>(type: "double precision", nullable: false),
                    Low10m = table.Column<decimal>(type: "numeric", nullable: false),
                    Low30m = table.Column<decimal>(type: "numeric", nullable: false),
                    Low60m = table.Column<decimal>(type: "numeric", nullable: false),
                    LowerWickPct10m = table.Column<double>(type: "double precision", nullable: false),
                    LowerWickPct30m = table.Column<double>(type: "double precision", nullable: false),
                    LowerWickPct60m = table.Column<double>(type: "double precision", nullable: false),
                    Momentum10m = table.Column<double>(type: "double precision", nullable: false),
                    Momentum30m = table.Column<double>(type: "double precision", nullable: false),
                    Momentum60m = table.Column<double>(type: "double precision", nullable: false),
                    NetPressure10m = table.Column<double>(type: "double precision", nullable: false),
                    NetPressure30m = table.Column<double>(type: "double precision", nullable: false),
                    NetPressure60m = table.Column<double>(type: "double precision", nullable: false),
                    OIChange = table.Column<double>(type: "double precision", nullable: false),
                    Open10m = table.Column<decimal>(type: "numeric", nullable: false),
                    Open30m = table.Column<decimal>(type: "numeric", nullable: false),
                    Open60m = table.Column<decimal>(type: "numeric", nullable: false),
                    Persistence10m = table.Column<double>(type: "double precision", nullable: false),
                    Persistence30m = table.Column<double>(type: "double precision", nullable: false),
                    Persistence60m = table.Column<double>(type: "double precision", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    RangeHigh10m = table.Column<decimal>(type: "numeric", nullable: false),
                    RangeHigh30m = table.Column<decimal>(type: "numeric", nullable: false),
                    RangeHigh60m = table.Column<decimal>(type: "numeric", nullable: false),
                    RangeLow10m = table.Column<decimal>(type: "numeric", nullable: false),
                    RangeLow30m = table.Column<decimal>(type: "numeric", nullable: false),
                    RangeLow60m = table.Column<decimal>(type: "numeric", nullable: false),
                    ScoreZ10m = table.Column<double>(type: "double precision", nullable: false),
                    ScoreZ30m = table.Column<double>(type: "double precision", nullable: false),
                    ScoreZ60m = table.Column<double>(type: "double precision", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    SnapshotTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Trend10m = table.Column<string>(type: "text", nullable: false),
                    Trend30m = table.Column<string>(type: "text", nullable: false),
                    Trend60m = table.Column<string>(type: "text", nullable: false),
                    UpperWickPct10m = table.Column<double>(type: "double precision", nullable: false),
                    UpperWickPct30m = table.Column<double>(type: "double precision", nullable: false),
                    UpperWickPct60m = table.Column<double>(type: "double precision", nullable: false),
                    VWAP = table.Column<decimal>(type: "numeric", nullable: false),
                    VelocityZ10m = table.Column<double>(type: "double precision", nullable: false),
                    VelocityZ30m = table.Column<double>(type: "double precision", nullable: false),
                    VelocityZ60m = table.Column<double>(type: "double precision", nullable: false)
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
                    ATRAtReview = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction15m = table.Column<string>(type: "text", nullable: false),
                    Direction30m = table.Column<string>(type: "text", nullable: false),
                    Direction60m = table.Column<string>(type: "text", nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Move15m = table.Column<decimal>(type: "numeric", nullable: false),
                    Move30m = table.Column<decimal>(type: "numeric", nullable: false),
                    Move60m = table.Column<decimal>(type: "numeric", nullable: false),
                    MoveATR15m = table.Column<double>(type: "double precision", nullable: false),
                    MoveATR30m = table.Column<double>(type: "double precision", nullable: false),
                    MoveATR60m = table.Column<double>(type: "double precision", nullable: false),
                    MovePct15m = table.Column<double>(type: "double precision", nullable: false),
                    MovePct30m = table.Column<double>(type: "double precision", nullable: false),
                    MovePct60m = table.Column<double>(type: "double precision", nullable: false),
                    Price15m = table.Column<decimal>(type: "numeric", nullable: false),
                    Price30m = table.Column<decimal>(type: "numeric", nullable: false),
                    Price60m = table.Column<decimal>(type: "numeric", nullable: false)
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
                    BehaviorEvidenceJson = table.Column<string>(type: "text", nullable: false),
                    ContradictionsJson = table.Column<string>(type: "text", nullable: false),
                    Correct15m = table.Column<bool>(type: "boolean", nullable: true),
                    Correct30m = table.Column<bool>(type: "boolean", nullable: true),
                    Correct60m = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DirectionalBias = table.Column<string>(type: "text", nullable: false),
                    DominantIntent = table.Column<string>(type: "text", nullable: false),
                    FlowQuality = table.Column<int>(type: "integer", nullable: false),
                    FutureMove15m = table.Column<decimal>(type: "numeric", nullable: true),
                    FutureMove30m = table.Column<decimal>(type: "numeric", nullable: true),
                    FutureMove60m = table.Column<decimal>(type: "numeric", nullable: true),
                    KeyDriversJson = table.Column<string>(type: "text", nullable: false),
                    LongConfidence = table.Column<int>(type: "integer", nullable: false),
                    MarketStructure = table.Column<string>(type: "text", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    RawResponse = table.Column<string>(type: "text", nullable: false),
                    RawResponseJson = table.Column<string>(type: "text", nullable: false),
                    RecommendedAction = table.Column<string>(type: "text", nullable: false),
                    RegimeQuality = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ShortConfidence = table.Column<int>(type: "integer", nullable: false),
                    StateAssessment = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Tradeability = table.Column<string>(type: "text", nullable: false),
                    TrendQuality = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_MarketPrices_TimestampUtc",
                table: "MarketPrices",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ModelReviews_SnapshotId",
                table: "ModelReviews",
                column: "SnapshotId");
        }
    }
}
