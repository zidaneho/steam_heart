using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamHeartAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGameTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReviewTable_GameId",
                table: "ReviewTable");

            migrationBuilder.DropIndex(
                name: "IX_MetricTable_GameId",
                table: "MetricTable");

            migrationBuilder.AddColumn<string>(
                name: "Developer",
                table: "GameTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "GameTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeaderImageUrl",
                table: "GameTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "GameTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReleaseDate",
                table: "GameTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Dictionary<string, int>>(
                name: "Tags",
                table: "GameTable",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Developer",
                table: "GameTable");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "GameTable");

            migrationBuilder.DropColumn(
                name: "HeaderImageUrl",
                table: "GameTable");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "GameTable");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "GameTable");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "GameTable");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTable_GameId",
                table: "ReviewTable",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricTable_GameId",
                table: "MetricTable",
                column: "GameId");
        }
    }
}
