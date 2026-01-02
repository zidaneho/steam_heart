using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SteamAlertsAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    RecommendationId = table.Column<string>(type: "text", nullable: false),
                    ReviewText = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    VotedUp = table.Column<bool>(type: "boolean", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimestampUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightedVoteScore = table.Column<double>(type: "double precision", nullable: false),
                    VotesUp = table.Column<int>(type: "integer", nullable: false),
                    VotesFunny = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    SteamPurchase = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivedForFree = table.Column<bool>(type: "boolean", nullable: false),
                    WrittenDuringEarlyAccess = table.Column<bool>(type: "boolean", nullable: false),
                    PrimarilySteamDeck = table.Column<bool>(type: "boolean", nullable: false),
                    ChangedVote = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorSteamId = table.Column<string>(type: "text", nullable: false),
                    AuthorNumGamesOwned = table.Column<int>(type: "integer", nullable: false),
                    AuthorNumReviews = table.Column<int>(type: "integer", nullable: false),
                    AuthorPlaytimeForever = table.Column<int>(type: "integer", nullable: false),
                    AuthorPlaytimeLastTwoWeeks = table.Column<int>(type: "integer", nullable: false),
                    AuthorPlaytimeAtReview = table.Column<int>(type: "integer", nullable: false),
                    AuthorLastPlayed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewTable_GameTable_GameId",
                        column: x => x.GameId,
                        principalTable: "GameTable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTable_GameId",
                table: "ReviewTable",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewTable");
        }
    }
}
