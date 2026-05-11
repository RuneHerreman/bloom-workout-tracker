using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddUserNamesAndWorkoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "LoggedWorkouts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "LoggedWorkouts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "LoggedWorkouts");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "LoggedWorkouts");
        }
    }
}
