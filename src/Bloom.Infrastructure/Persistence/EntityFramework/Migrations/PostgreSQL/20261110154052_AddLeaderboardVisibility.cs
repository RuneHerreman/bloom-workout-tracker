using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddLeaderboardVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowOnLeaderboard",
                table: "Users",
                type: "boolean",
                nullable: false,
                // Opt-out: existing users stay visible, matching the behaviour before this
                // setting existed. Only backfills existing rows; the app always writes the value.
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowOnLeaderboard",
                table: "Users");
        }
    }
}
