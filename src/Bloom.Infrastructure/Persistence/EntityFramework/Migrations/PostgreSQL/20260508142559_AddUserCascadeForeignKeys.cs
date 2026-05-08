using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddUserCascadeForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplates_UserId",
                table: "WorkoutTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoggedWorkouts_UserId",
                table: "LoggedWorkouts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoggedWorkouts_Users_UserId",
                table: "LoggedWorkouts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutTemplates_Users_UserId",
                table: "WorkoutTemplates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoggedWorkouts_Users_UserId",
                table: "LoggedWorkouts");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutTemplates_Users_UserId",
                table: "WorkoutTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutTemplates_UserId",
                table: "WorkoutTemplates");

            migrationBuilder.DropIndex(
                name: "IX_LoggedWorkouts_UserId",
                table: "LoggedWorkouts");
        }
    }
}
