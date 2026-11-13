using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <summary>
    /// One muscle list per exercise, from the muscle tree (areas with muscles in them).
    /// Secondary muscles are folded into the list, older names map onto the tree
    /// (Upper back and Rhomboids to Mid back, Quadriceps to Quads) and "Full Body" becomes
    /// every area. Catalog exercises are retagged exactly by the seeder afterwards.
    /// </summary>
    public partial class MuscleTree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Exercises" e
                SET "TargetMuscles" = (
                    SELECT COALESCE(jsonb_agg(jsonb_build_object('Value', v) ORDER BY first), '[]'::jsonb)
                    FROM (
                        SELECT v, min(ord * 100 + sub) AS first
                        FROM (
                            SELECT u.v, t.ord, u.sub
                            FROM jsonb_array_elements(
                                     CASE WHEN jsonb_typeof(e."TargetMuscles") = 'array' THEN e."TargetMuscles" ELSE '[]'::jsonb END
                                     || CASE WHEN jsonb_typeof(e."SecondaryMuscles") = 'array' THEN e."SecondaryMuscles" ELSE '[]'::jsonb END
                                 ) WITH ORDINALITY AS t(el, ord),
                                 LATERAL (SELECT el->>'Value' AS m) AS x,
                                 LATERAL unnest(CASE
                                       WHEN lower(m) IN ('full body', 'whole body') THEN ARRAY['Chest', 'Back', 'Shoulders', 'Arms', 'Core', 'Legs']
                                       WHEN lower(m) IN ('upper back', 'rhomboids') THEN ARRAY['Mid back']
                                       WHEN lower(m) = 'quadriceps' THEN ARRAY['Quads']
                                       WHEN lower(m) IN ('upper chest', 'lower chest') THEN ARRAY['Chest']
                                       WHEN lower(m) = 'trapezius' THEN ARRAY['Traps']
                                       ELSE ARRAY[m]
                                   END) WITH ORDINALITY AS u(v, sub)
                            WHERE m IS NOT NULL AND btrim(m) <> ''
                        ) mapped
                        GROUP BY v
                    ) deduped
                );
                """);

            migrationBuilder.DropColumn(
                name: "SecondaryMuscles",
                table: "Exercises");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lossy: the merged list stays as the target muscles.
            migrationBuilder.AddColumn<string>(
                name: "SecondaryMuscles",
                table: "Exercises",
                type: "jsonb",
                nullable: true);
        }
    }
}
