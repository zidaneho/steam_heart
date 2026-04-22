using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamHeartAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // GameTable: AppId is used in nearly every game lookup
            migrationBuilder.CreateIndex(
                name: "IX_GameTable_AppId",
                table: "GameTable",
                column: "AppId",
                unique: true);

            // MetricTable: composite covers "all metrics for a game ordered by time"
            // and makes the existing standalone GameId index redundant
            migrationBuilder.CreateIndex(
                name: "IX_MetricTable_GameId_Timestamp",
                table: "MetricTable",
                columns: new[] { "GameId", "Timestamp" });

            // ReviewTable: unique dedup key used during sync
            migrationBuilder.CreateIndex(
                name: "IX_ReviewTable_RecommendationId",
                table: "ReviewTable",
                column: "RecommendationId",
                unique: true);

            // ReviewTable: composite for "all reviews for a game sorted by date"
            // covers the existing standalone GameId index
            migrationBuilder.CreateIndex(
                name: "IX_ReviewTable_GameId_TimestampCreated",
                table: "ReviewTable",
                columns: new[] { "GameId", "TimestampCreated" });

            // ReviewTable: standalone filter columns for analytics queries
            migrationBuilder.CreateIndex(
                name: "IX_ReviewTable_Language",
                table: "ReviewTable",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTable_VotedUp",
                table: "ReviewTable",
                column: "VotedUp");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTable_AuthorSteamId",
                table: "ReviewTable",
                column: "AuthorSteamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_GameTable_AppId",                    table: "GameTable");
            migrationBuilder.DropIndex(name: "IX_MetricTable_GameId_Timestamp",        table: "MetricTable");
            migrationBuilder.DropIndex(name: "IX_ReviewTable_RecommendationId",        table: "ReviewTable");
            migrationBuilder.DropIndex(name: "IX_ReviewTable_GameId_TimestampCreated", table: "ReviewTable");
            migrationBuilder.DropIndex(name: "IX_ReviewTable_Language",                table: "ReviewTable");
            migrationBuilder.DropIndex(name: "IX_ReviewTable_VotedUp",                 table: "ReviewTable");
            migrationBuilder.DropIndex(name: "IX_ReviewTable_AuthorSteamId",           table: "ReviewTable");
        }
    }
}
