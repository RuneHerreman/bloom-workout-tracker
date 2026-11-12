using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddGearIcons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GearIcons",
                table: "Users",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GearIcons",
                table: "Users");
        }
    }
}
