using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddGearNotesSetMarkersAndCustomExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gear",
                table: "Users",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "Exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_OwnerUserId",
                table: "Exercises",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_OwnerUserId",
                table: "Exercises",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_OwnerUserId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_OwnerUserId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Gear",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Exercises");
        }
    }
}
