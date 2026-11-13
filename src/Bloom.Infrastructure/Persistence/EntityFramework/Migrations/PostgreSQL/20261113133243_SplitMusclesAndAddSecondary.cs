using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bloom.Infrastructure.Persistence.EntityFramework.Migrations.PostgreSQL
{
    /// <summary>
    /// Adds secondary muscles and renames broad muscle tags to finer ones: Back becomes
    /// Lats / Upper back / Lower back, Shoulders becomes Front / Side / Rear delts, Core
    /// becomes Abs / Obliques and Rhomboids folds into Upper back. The part is guessed from
    /// the exercise name. Catalog exercises are retagged exactly by the seeder afterwards;
    /// this mainly keeps users' custom exercises on names the app knows.
    /// </summary>
    public partial class SplitMusclesAndAddSecondary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecondaryMuscles",
                table: "Exercises",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql("""UPDATE "Exercises" SET "SecondaryMuscles" = '[]'::jsonb WHERE "SecondaryMuscles" IS NULL;""");

            migrationBuilder.Sql(RenameSql("""
                CASE
                    WHEN lower(m) = 'rhomboids' THEN 'Upper back'
                    WHEN lower(m) = 'back' AND name ~* '(pulldown|pull[ -]?up|chin|pullover|straight arm)' THEN 'Lats'
                    WHEN lower(m) = 'back' AND name ~* '(deadlift|extension|hyper|good morning|rack pull|superman)' THEN 'Lower back'
                    WHEN lower(m) = 'back' THEN 'Upper back'
                    WHEN lower(m) = 'shoulders' AND name ~* '(lateral|side|upright|lu raise)' THEN 'Side delts'
                    WHEN lower(m) = 'shoulders' AND name ~* '(rear|reverse|face pull|pull apart)' THEN 'Rear delts'
                    WHEN lower(m) = 'shoulders' THEN 'Front delts'
                    WHEN lower(m) = 'core' AND name ~* '(oblique|twist|side plank|woodchop|pallof|suitcase)' THEN 'Obliques'
                    WHEN lower(m) = 'core' THEN 'Abs'
                    ELSE m
                END
                """, "'back','shoulders','core','rhomboids'"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RenameSql("""
                CASE
                    WHEN m IN ('Front delts','Side delts','Rear delts') THEN 'Shoulders'
                    WHEN m IN ('Upper back','Lower back') THEN 'Back'
                    WHEN m IN ('Abs','Obliques') THEN 'Core'
                    ELSE m
                END
                """, "'front delts','side delts','rear delts','upper back','lower back','abs','obliques'"));

            migrationBuilder.DropColumn(
                name: "SecondaryMuscles",
                table: "Exercises");
        }

        /// <summary>
        /// Rewrites every TargetMuscles entry through <paramref name="mapping"/> (an SQL
        /// expression over <c>m</c>, the muscle, and <c>name</c>, the exercise name), keeping
        /// order and dropping duplicates. Only rows holding one of <paramref name="oldNames"/>
        /// (lower case, quoted) are touched.
        /// </summary>
        private static string RenameSql(string mapping, string oldNames) => $"""
            UPDATE "Exercises" e
            SET "TargetMuscles" = (
                SELECT COALESCE(jsonb_agg(jsonb_build_object('Value', v) ORDER BY first), '[]'::jsonb)
                FROM (
                    SELECT v, min(ord) AS first
                    FROM (
                        SELECT ({mapping}) AS v, ord
                        FROM jsonb_array_elements(e."TargetMuscles") WITH ORDINALITY AS t(el, ord),
                             LATERAL (SELECT el->>'Value' AS m, e."Name" AS name) AS x
                    ) mapped
                    GROUP BY v
                ) deduped
            )
            WHERE jsonb_typeof(e."TargetMuscles") = 'array'
              AND EXISTS (
                  SELECT 1 FROM jsonb_array_elements(e."TargetMuscles") el
                  WHERE lower(el->>'Value') IN ({oldNames})
              );
            """;
    }
}
