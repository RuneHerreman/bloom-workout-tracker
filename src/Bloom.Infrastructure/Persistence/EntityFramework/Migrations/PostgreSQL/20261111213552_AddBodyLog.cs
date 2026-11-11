using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddBodyLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Users",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "BodyMeasurementIds",
                table: "ImportBatches",
                type: "uuid[]",
                nullable: false,
                // Existing imports added no body entries.
                defaultValueSql: "'{}'::uuid[]");

            migrationBuilder.CreateTable(
                name: "BodyMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    BodyFatPct = table.Column<decimal>(type: "numeric(4,1)", nullable: true),
                    Source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BodyMeasurements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BodyMeasurements_UserId_Date",
                table: "BodyMeasurements",
                columns: new[] { "UserId", "Date" },
                unique: true);

            // Every existing user starts their body log with the weight on their profile.
            // Users have no created-at column, so it's dated to their first logged workout,
            // or today when they have none.
            migrationBuilder.Sql("""
                INSERT INTO "BodyMeasurements" ("Id", "UserId", "Date", "WeightKg", "BodyFatPct", "Source", "CreatedAt")
                SELECT gen_random_uuid(),
                       u."Id",
                       COALESCE((SELECT MIN(lw."LoggedAt")::date FROM "LoggedWorkouts" lw WHERE lw."UserId" = u."Id"), CURRENT_DATE),
                       u."Weight",
                       NULL,
                       'Signup',
                       now()
                FROM "Users" u
                WHERE u."Weight" BETWEEN 20 AND 400;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodyMeasurements");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BodyMeasurementIds",
                table: "ImportBatches");
        }
    }
}
