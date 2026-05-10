using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Infrastructure.Auth;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Seeders;

public class DomainDbSeeder(DomainDbContext context, ILogger<DomainDbSeeder> logger, IPasswordHasher passwordHasher)
{
    private static readonly UserId SeededUserId = EntityId.New<UserId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490000"));

    public async Task Seed()
    {
        await SeedExercises();
        await SeedUsers();
        await SeedTemplates();
        await SeedLogs();
    }

    private async Task SeedUsers()
    {
        if (await context.Users.AnyAsync())
            return;

        var users = new List<User>
        {
            User.Create("frans.appelmans@gmail.com", "FransAppelmans", passwordHasher.HashPassword("test"), 80m, 180, 3, EntityId.New<UserId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490000"))),
        };
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private async Task SeedExercises()
    {
        if (await context.Exercises.AnyAsync())
            return;

        var exercises = new List<Exercise>
        {
            // Strength
            Exercise.Create("Bench Press", "A compound push exercise targeting the chest, shoulders and triceps.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490001"))),
            Exercise.Create("Incline Bench Press", "A compound push exercise targeting the upper chest.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490002"))),
            Exercise.Create("Decline Bench Press", "A compound push exercise targeting the lower chest.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490003"))),
            Exercise.Create("Dumbbell Flye", "An isolation exercise targeting the chest.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490004"))),
            Exercise.Create("Cable Crossover", "An isolation exercise targeting the chest using cables.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490005"))),
            Exercise.Create("Overhead Press", "A compound push exercise targeting the shoulders and triceps.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490006"))),
            Exercise.Create("Lateral Raise", "An isolation exercise targeting the lateral deltoid.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490007"))),
            Exercise.Create("Front Raise", "An isolation exercise targeting the front deltoid.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490008"))),
            Exercise.Create("Reverse Flye", "An isolation exercise targeting the rear deltoid.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490009"))),
            Exercise.Create("Arnold Press", "A compound shoulder press with rotation targeting all three deltoid heads.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490010"))),
            Exercise.Create("Barbell Row", "A compound pull exercise targeting the back and biceps.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490011"))),
            Exercise.Create("Deadlift", "A compound hinge exercise targeting the posterior chain.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490012"))),
            Exercise.Create("Lat Pulldown", "A compound pull exercise targeting the latissimus dorsi.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490013"))),
            Exercise.Create("Seated Cable Row", "A compound pull exercise targeting the mid back.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490014"))),
            Exercise.Create("Single Arm Dumbbell Row", "A unilateral compound pull exercise targeting the back.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490015"))),
            Exercise.Create("Barbell Curl", "An isolation exercise targeting the biceps.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490016"))),
            Exercise.Create("Dumbbell Curl", "An isolation exercise targeting the biceps.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490017"))),
            Exercise.Create("Hammer Curl", "An isolation exercise targeting the biceps and brachialis.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490018"))),
            Exercise.Create("Preacher Curl", "An isolation exercise targeting the biceps with reduced momentum.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490019"))),
            Exercise.Create("Cable Curl", "An isolation exercise targeting the biceps using cables.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490020"))),
            Exercise.Create("Tricep Pushdown", "An isolation exercise targeting the triceps using a cable.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490021"))),
            Exercise.Create("Skull Crusher", "An isolation exercise targeting the triceps with a barbell or dumbbells.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490022"))),
            Exercise.Create("Overhead Tricep Extension", "An isolation exercise targeting the long head of the triceps.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490023"))),
            Exercise.Create("Close Grip Bench Press", "A compound push exercise targeting the triceps and chest.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490024"))),
            Exercise.Create("Tricep Kickback", "An isolation exercise targeting the triceps.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490025"))),
            Exercise.Create("Squat", "A compound lower body exercise targeting the quads, glutes and hamstrings.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490026"))),
            Exercise.Create("Leg Press", "A compound lower body exercise targeting the quads and glutes.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490027"))),
            Exercise.Create("Leg Extension", "An isolation exercise targeting the quadriceps.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490028"))),
            Exercise.Create("Bulgarian Split Squat", "A unilateral compound exercise targeting the quads and glutes.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490029"))),
            Exercise.Create("Hack Squat", "A compound lower body exercise targeting the quads.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490030"))),
            Exercise.Create("Romanian Deadlift", "A compound hinge exercise targeting the hamstrings and glutes.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490031"))),
            Exercise.Create("Leg Curl", "An isolation exercise targeting the hamstrings.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490032"))),
            Exercise.Create("Nordic Curl", "A bodyweight exercise targeting the hamstrings eccentrically.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490033"))),
            Exercise.Create("Good Morning", "A compound hinge exercise targeting the hamstrings and lower back.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490034"))),
            Exercise.Create("Hip Thrust", "A compound exercise targeting the glutes.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490035"))),
            Exercise.Create("Cable Kickback", "An isolation exercise targeting the glutes.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490036"))),
            Exercise.Create("Standing Calf Raise", "An isolation exercise targeting the gastrocnemius.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490037"))),
            Exercise.Create("Seated Calf Raise", "An isolation exercise targeting the soleus.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490038"))),
            Exercise.Create("Plank", "An isometric core exercise targeting the abdominals and stabilisers.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490039"))),
            Exercise.Create("Cable Crunch", "An isolation exercise targeting the abdominals.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490040"))),
            Exercise.Create("Hanging Leg Raise", "A compound core exercise targeting the abdominals and hip flexors.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490041"))),
            Exercise.Create("Farmers Carry", "A functional strength exercise targeting the forearms, traps and core.", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490042"))),
            Exercise.Create("Wrist Curl", "An isolation exercise targeting the forearm flexors.", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490043"))),
            Exercise.Create("Shrug", "An isolation exercise targeting the trapezius.", ExerciseType.Strength, ["Traps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490044"))),
            Exercise.Create("Face Pull", "A compound exercise targeting the rear deltoids and external rotators.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490045"))),
            Exercise.Create("Pull Up", "A compound pull exercise targeting the back and biceps.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490046"))),
            Exercise.Create("Dip", "A compound push exercise targeting the chest and triceps.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490047"))),
            Exercise.Create("Lunge", "A unilateral compound exercise targeting the quads and glutes.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490048"))),
            Exercise.Create("Sumo Deadlift", "A wide stance deadlift variation targeting the inner thighs and glutes.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490049"))),
            Exercise.Create("Incline Dumbbell Curl", "An isolation exercise targeting the biceps with a stretch at the bottom.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490050"))),

            // Cardio
Exercise.Create("Treadmill Run", "A sustained cardiovascular exercise performed on a treadmill.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490051"))),
Exercise.Create("Cycling", "A low impact cardiovascular exercise performed on a bike.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490052"))),
Exercise.Create("Rowing Machine", "A full body cardiovascular exercise performed on a rowing ergometer.", ExerciseType.Cardio, ["Lats", "Rhomboids", "Quadriceps", "Hamstrings", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490053"))),
Exercise.Create("Stair Climber", "A cardiovascular exercise targeting the lower body using a stair machine.", ExerciseType.Cardio, ["Glutes", "Quadriceps", "Hamstrings", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490054"))),
Exercise.Create("Elliptical", "A low impact full body cardiovascular exercise.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Hamstrings", "Shoulders", "Chest", "Lats"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490055"))),
Exercise.Create("Jump Rope", "A high intensity cardiovascular exercise using a skipping rope.", ExerciseType.Cardio, ["Calves", "Quadriceps", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490056"))),
Exercise.Create("Swimming", "A full body low impact cardiovascular exercise performed in water.", ExerciseType.Cardio, ["Lats", "Shoulders", "Chest", "Core", "Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490057"))),
Exercise.Create("Battle Ropes", "A high intensity cardiovascular exercise using heavy ropes.", ExerciseType.Cardio, ["Shoulders", "Core", "Lats", "Biceps", "Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490058"))),
Exercise.Create("Assault Bike", "A full body high intensity cardiovascular exercise on an air bike.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Shoulders", "Chest", "Lats", "Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490059"))),
Exercise.Create("Sled Push", "A functional cardiovascular exercise pushing a weighted sled.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Calves", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490060"))),
            // Plyometric
            Exercise.Create("Box Jump", "An explosive lower body exercise jumping onto a raised platform.", ExerciseType.Plyometric, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490061"))),
            Exercise.Create("Depth Jump", "An advanced plyometric exercise stepping off a box and immediately jumping.", ExerciseType.Plyometric, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490062"))),
            Exercise.Create("Broad Jump", "An explosive horizontal jump targeting the lower body.", ExerciseType.Plyometric, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490063"))),
            Exercise.Create("Lateral Bound", "An explosive lateral jump targeting the glutes and abductors.", ExerciseType.Plyometric, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490064"))),
            Exercise.Create("Clap Push Up", "An explosive upper body plyometric exercise.", ExerciseType.Plyometric, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490065"))),
            Exercise.Create("Tuck Jump", "An explosive jump bringing the knees to the chest at the top.", ExerciseType.Plyometric, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490066"))),
            Exercise.Create("Burpee", "A full body plyometric exercise combining a squat, push up and jump.", ExerciseType.Plyometric, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490067"))),
            Exercise.Create("Medicine Ball Slam", "An explosive full body exercise slamming a medicine ball to the ground.", ExerciseType.Plyometric, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490068"))),
            Exercise.Create("Single Leg Hop", "A unilateral plyometric exercise targeting the quads and glutes.", ExerciseType.Plyometric, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490069"))),
            Exercise.Create("Hurdle Jump", "An explosive jump over a hurdle targeting the lower body.", ExerciseType.Plyometric, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490070"))),
    
        };

        context.Exercises.AddRange(exercises);
        await context.SaveChangesAsync();
    }

    private async Task SeedTemplates()
    {
        if (await context.WorkoutTemplates.AnyAsync())
            return;

        // Exercise IDs from seeded exercises
        var benchPress      = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490001"));
        var inclineBench    = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490002"));
        var overheadPress   = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490006"));
        var lateralRaise    = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490007"));
        var tricepPushdown  = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490021"));
        var barbellRow      = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490011"));
        var latPulldown     = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490013"));
        var seatedCableRow  = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490014"));
        var barbellCurl     = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490016"));
        var hammerCurl      = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490018"));
        var squat           = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490026"));
        var legPress        = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490027"));
        var romanianDl      = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490031"));
        var legCurl         = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490032"));
        var calfRaise       = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490037"));

        var templates = new List<WorkoutTemplate>
        {
            WorkoutTemplate.Create(SeededUserId, "Push", [
                TemplateExercise.Create(benchPress, 1, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 8),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 8),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 6),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 4, 6),
                ]),
                TemplateExercise.Create(inclineBench, 2, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 10),
                ]),
                TemplateExercise.Create(overheadPress, 3, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 10),
                ]),
                TemplateExercise.Create(lateralRaise, 4, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 15),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 15),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 15),
                ]),
                TemplateExercise.Create(tricepPushdown, 5, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 12),
                ]),
            ]),

            WorkoutTemplate.Create(SeededUserId, "Pull", [
                TemplateExercise.Create(barbellRow, 1, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 8),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 8),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 8),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 4, 8),
                ]),
                TemplateExercise.Create(latPulldown, 2, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 10),
                ]),
                TemplateExercise.Create(seatedCableRow, 3, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 12),
                ]),
                TemplateExercise.Create(barbellCurl, 4, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 10),
                ]),
                TemplateExercise.Create(hammerCurl, 5, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 12),
                ]),
            ]),

            WorkoutTemplate.Create(SeededUserId, "Legs", [
                TemplateExercise.Create(squat, 1, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 6),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 6),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 6),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 4, 6),
                ]),
                TemplateExercise.Create(legPress, 2, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 10),
                ]),
                TemplateExercise.Create(romanianDl, 3, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 10),
                ]),
                TemplateExercise.Create(legCurl, 4, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 12),
                ]),
                TemplateExercise.Create(calfRaise, 5, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 15),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 15),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 15),
                ]),
            ]),
        };

        context.WorkoutTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }

    private async Task SeedLogs()
    {
        if (await context.LoggedWorkouts.AnyAsync())
            return;

        var benchPress  = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490001"));
        var overheadPress = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490006"));
        var tricepPushdown = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490021"));
        var barbellRow  = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490011"));
        var latPulldown = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490013"));
        var barbellCurl = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490016"));
        var squat       = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490026"));
        var romanianDl  = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490031"));
        var legCurl     = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490032"));
        var treadmill   = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490051"));
        var boxJump     = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490061"));

        var today = DateTime.UtcNow.Date;

        var logs = new List<LoggedWorkout>
        {
            // Week -1: Push / Pull / Legs
            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(benchPress, 1, [
                    LoggedSet.CreateStrength(1, 8, 90m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 92.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 6, 95m, WeightUnit.Kg, 1),
                    LoggedSet.CreateStrength(4, 6, 97.5m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(overheadPress, 2, [
                    LoggedSet.CreateStrength(1, 10, 55m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 57.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 60m, WeightUnit.Kg, 1),
                ]),
                LoggedExercise.Create(tricepPushdown, 3, [
                    LoggedSet.CreateStrength(1, 12, 35m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 12, 37.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 10, 40m, WeightUnit.Kg, 1),
                ]),
            ], today.AddDays(-2)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(barbellRow, 1, [
                    LoggedSet.CreateStrength(1, 8, 75m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 77.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 80m, WeightUnit.Kg, 1),
                    LoggedSet.CreateStrength(4, 6, 82.5m, WeightUnit.Kg, 1),
                ]),
                LoggedExercise.Create(latPulldown, 2, [
                    LoggedSet.CreateStrength(1, 10, 65m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 67.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 10, 70m, WeightUnit.Kg, 1),
                ]),
                LoggedExercise.Create(barbellCurl, 3, [
                    LoggedSet.CreateStrength(1, 10, 32.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 35m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 37.5m, WeightUnit.Kg, 1),
                ]),
            ], today.AddDays(-4)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(squat, 1, [
                    LoggedSet.CreateStrength(1, 6, 105m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 6, 107.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 6, 110m, WeightUnit.Kg, 1),
                    LoggedSet.CreateStrength(4, 5, 112.5m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(romanianDl, 2, [
                    LoggedSet.CreateStrength(1, 10, 80m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 82.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 85m, WeightUnit.Kg, 1),
                ]),
                LoggedExercise.Create(legCurl, 3, [
                    LoggedSet.CreateStrength(1, 12, 45m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 12, 47.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 10, 50m, WeightUnit.Kg, 1),
                ]),
            ], today.AddDays(-6)),

            // Week -2
            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(benchPress, 1, [
                    LoggedSet.CreateStrength(1, 8, 87.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 90m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 6, 92.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(4, 6, 95m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(overheadPress, 2, [
                    LoggedSet.CreateStrength(1, 10, 52.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 55m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 57.5m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-9)),

            // Cardio session
            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(treadmill, 1, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromMinutes(30), 5.0m, DistanceUnit.Km),
                ]),
            ], today.AddDays(-11)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(squat, 1, [
                    LoggedSet.CreateStrength(1, 6, 100m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 6, 102.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 6, 105m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(4, 5, 107.5m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(romanianDl, 2, [
                    LoggedSet.CreateStrength(1, 10, 77.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 80m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 82.5m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-13)),

            // Week -3: Push with plyo finisher (mixed)
            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(benchPress, 1, [
                    LoggedSet.CreateStrength(1, 8, 85m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 87.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 6, 90m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(boxJump, 2, [
                    LoggedSet.CreatePlyometric(1, 5, 0m, WeightUnit.Kg, 0),
                    LoggedSet.CreatePlyometric(2, 5, 0m, WeightUnit.Kg, 0),
                    LoggedSet.CreatePlyometric(3, 5, 0m, WeightUnit.Kg, 0),
                ]),
            ], today.AddDays(-16)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(barbellRow, 1, [
                    LoggedSet.CreateStrength(1, 8, 72.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 75m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 77.5m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(latPulldown, 2, [
                    LoggedSet.CreateStrength(1, 10, 62.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 65m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 10, 67.5m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-18)),

            // Cardio
            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(treadmill, 1, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromMinutes(25), 4.0m, DistanceUnit.Km),
                ]),
            ], today.AddDays(-20)),

            // Week -4 and beyond
            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(benchPress, 1, [
                    LoggedSet.CreateStrength(1, 8, 82.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 85m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 6, 87.5m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(overheadPress, 2, [
                    LoggedSet.CreateStrength(1, 10, 50m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 52.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 55m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-23)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(squat, 1, [
                    LoggedSet.CreateStrength(1, 6, 97.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 6, 100m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 5, 102.5m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(romanianDl, 2, [
                    LoggedSet.CreateStrength(1, 10, 75m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 77.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 80m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-25)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(treadmill, 1, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromMinutes(35), 5.5m, DistanceUnit.Km),
                ]),
            ], today.AddDays(-27)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(barbellRow, 1, [
                    LoggedSet.CreateStrength(1, 8, 70m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 72.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 75m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(barbellCurl, 2, [
                    LoggedSet.CreateStrength(1, 10, 30m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 10, 32.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 8, 35m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-30)),

            // Historic logs for Volume Chart trends
            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(benchPress, 1, [
                    LoggedSet.CreateStrength(1, 8, 75m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 77.5m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(3, 6, 80m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(overheadPress, 2, [
                    LoggedSet.CreateStrength(1, 10, 45m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-45)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(squat, 1, [
                    LoggedSet.CreateStrength(1, 6, 85m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 6, 90m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(romanianDl, 2, [
                    LoggedSet.CreateStrength(1, 10, 65m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-50)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(barbellRow, 1, [
                    LoggedSet.CreateStrength(1, 8, 65m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 67.5m, WeightUnit.Kg, 2),
                ]),
                LoggedExercise.Create(latPulldown, 2, [
                    LoggedSet.CreateStrength(1, 10, 55m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-60)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(benchPress, 1, [
                    LoggedSet.CreateStrength(1, 8, 70m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 72.5m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-75)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(squat, 1, [
                    LoggedSet.CreateStrength(1, 6, 80m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 6, 82.5m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-80)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(barbellRow, 1, [
                    LoggedSet.CreateStrength(1, 8, 60m, WeightUnit.Kg, 2),
                    LoggedSet.CreateStrength(2, 8, 62.5m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-90)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(benchPress, 1, [
                    LoggedSet.CreateStrength(1, 8, 65m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-105)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(squat, 1, [
                    LoggedSet.CreateStrength(1, 6, 70m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-120)),

            LoggedWorkout.Create(SeededUserId, [
                LoggedExercise.Create(barbellRow, 1, [
                    LoggedSet.CreateStrength(1, 8, 55m, WeightUnit.Kg, 2),
                ]),
            ], today.AddDays(-135)),
        };

        context.LoggedWorkouts.AddRange(logs);
        await context.SaveChangesAsync();
    }
}
