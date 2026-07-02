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
using Bloom.Domain.WorkoutTemplates.ValueObjects;
using Bloom.Infrastructure.Auth;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Seeders;

public class DomainDbSeeder(DomainDbContext context, ILogger<DomainDbSeeder> logger, IPasswordHasher passwordHasher)
{
    private static readonly UserId SeededUserId = EntityId.New<UserId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490000"));

    /// <summary>Seeds data required in every environment: the exercise catalog.</summary>
    public async Task SeedProductionData()
    {
        await SeedExercises();
    }

    /// <summary>Seeds development-only test data: users, templates and logs.</summary>
    public async Task SeedDevelopmentData()
    {
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
            User.Create("frans.appelmans@gmail.com", "FransAppelmans", passwordHasher.HashPassword("test"), "Frans", "Appelmans", 80m, 180, 3, new DateOnly(1990, 1, 1), EntityId.New<UserId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490000"))),
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private async Task SeedExercises()
    {
        var existingIds = await context.Exercises.Select(e => e.Id.Value).ToListAsync();
        var existingSet = existingIds.ToHashSet();

        var allExercises = new List<Exercise>
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

            // New exercises
            Exercise.Create("Hip Adductor", "A machine isolation exercise targeting the inner thighs.", ExerciseType.Strength, ["Adductors"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490071"))),
            Exercise.Create("Hip Abductor", "A machine isolation exercise targeting the outer thighs and glutes.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490072"))),
            Exercise.Create("Sit Up", "A core exercise targeting the abdominals.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490073"))),
            Exercise.Create("Chest Fly", "An isolation exercise targeting the chest using a fly machine or dumbbells.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490074"))),
            Exercise.Create("Bouldering", "An indoor rock climbing sport targeting the full body, especially grip and upper body.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490075"))),
            Exercise.Create("Indoor Snowboarding", "An indoor snowboarding session targeting the legs, core and balance.", ExerciseType.Cardio, ["Quadriceps", "Core", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490076"))),
            Exercise.Create("Incline Dumbbell Press", "A compound push exercise targeting the upper chest using dumbbells on an incline bench.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490077"))),
            Exercise.Create("Machine Shoulder Press", "A compound push exercise targeting the shoulders using a shoulder press machine.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490078"))),
            Exercise.Create("Chest Press", "A compound push exercise targeting the chest using a chest press machine.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490079"))),
            Exercise.Create("Machine Row", "A compound pull exercise targeting the back using a rowing machine.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490080"))),
            Exercise.Create("T-bar Row", "A compound pull exercise targeting the back using a T-bar setup.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490081"))),
            Exercise.Create("Chest Supported Row", "A compound pull exercise targeting the back performed on an incline bench for chest support.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490082"))),
            Exercise.Create("Cable Hammer Curl", "An isolation exercise targeting the biceps and brachialis using a cable machine.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490083"))),
            Exercise.Create("Forearm Curl", "An isolation exercise targeting the forearm flexors with a barbell or cable.", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490084"))),
            Exercise.Create("Calf Press", "An isolation exercise targeting the calves performed on a leg press machine.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490085"))),
            Exercise.Create("Machine Crunch", "An isolation exercise targeting the abdominals using a crunch machine.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490086"))),
            Exercise.Create("Crunch", "A bodyweight isolation exercise targeting the abdominals.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490087"))),
            Exercise.Create("Bayesian Curl", "An isolation exercise targeting the biceps using a cable set behind the body for a full stretch.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490088"))),
            Exercise.Create("Reverse Forearm Curl", "An isolation exercise targeting the forearm extensors with a reverse grip.", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490089"))),
            

            // ——————————————————————————————————————————————
            // CHEST — machine / dumbbell / cable / bodyweight variants
            // ——————————————————————————————————————————————
            Exercise.Create("Dumbbell Bench Press", "A compound horizontal press using dumbbells, allowing a greater range of motion and independent arm movement compared to the barbell version, which helps address strength imbalances and increases pec stretch at the bottom.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490302"))),
            Exercise.Create("Machine Chest Fly", "A pec-deck or fly machine exercise that keeps the arms in a fixed arc, providing constant tension on the chest through the full range of motion without the stabilisation demands of free weights.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490303"))),
            Exercise.Create("Low Cable Fly", "A cable fly performed with the pulleys set low and hands sweeping upward, emphasising the clavicular (upper) fibres of the pectoralis major while maintaining constant cable tension throughout the movement.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490304"))),
            Exercise.Create("Push Up", "A bodyweight horizontal push exercise where the hands support the body in a plank position and the body is lowered to the floor, targeting the chest, front delts and triceps with significant core stabilisation demands.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490305"))),
            Exercise.Create("Landmine Press", "A pressing movement using a barbell anchored at one end, angled roughly 45 degrees, providing a shoulder-friendly arc that targets the upper chest and front delts while allowing heavy loading with minimal joint stress.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490306"))),
            Exercise.Create("Decline Dumbbell Press", "A dumbbell press performed on a decline bench (15-30 degrees), shifting emphasis to the sternal (lower) head of the pectoralis major while allowing independent arm movement and a deeper stretch.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490307"))),
            Exercise.Create("Smith Machine Bench Press", "A horizontal press on the Smith machine, which constrains the bar to a vertical path, reducing stabiliser demand and allowing focus on chest activation — useful for training to failure without a spotter.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490308"))),
            Exercise.Create("Incline Cable Fly", "A cable fly with an incline bench positioned between two pulleys set low, combining the constant tension of cables with an upward pressing arc that biases the upper pectorals.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490309"))),
            Exercise.Create("Dumbbell Pullover", "A single-joint movement lying perpendicular on a bench, lowering a dumbbell behind the head in an arc, stretching and loading the pecs and lats through a long range of motion while expanding the ribcage.", ExerciseType.Strength, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490310"))),
            Exercise.Create("Weight Training", "Place holder for strava weight training", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490311"))),

            // ——————————————————————————————————————————————
            // SHOULDERS — machine / dumbbell / cable / barbell variants
            // ——————————————————————————————————————————————
            Exercise.Create("Dumbbell Shoulder Press", "A seated or standing overhead press with dumbbells, allowing each arm to move independently and follow a natural arc, which can reduce shoulder impingement risk compared to a fixed barbell path.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490090"))),
            Exercise.Create("Cable Lateral Raise", "A lateral raise performed with a low cable, providing increasing tension through the concentric phase and a more consistent resistance profile than dumbbells, which lose tension at the bottom.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490091"))),
            Exercise.Create("Cable Front Raise", "A front deltoid isolation exercise using a low cable, keeping constant tension on the anterior deltoid through the full range, unlike dumbbells which lose resistance at the bottom.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490092"))),
            Exercise.Create("Machine Lateral Raise", "A lateral raise performed on a machine with elbow pads, removing grip as a limiting factor and isolating the medial deltoid through a fixed arc of motion.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490093"))),
            Exercise.Create("Upright Row", "A vertical pull using a barbell, dumbbells or cable, drawing the weight up along the torso to chin height, targeting the lateral deltoids and upper traps — best performed with a wide grip to minimise shoulder impingement.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490094"))),
            Exercise.Create("Cable Reverse Fly", "A rear deltoid isolation exercise performed by pulling two cables set at face height outward in a reverse hugging motion, maintaining constant tension on the posterior deltoid and rhomboids throughout.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490095"))),
            Exercise.Create("Behind The Neck Press", "An overhead barbell press lowered behind the head to ear level, increasing lateral deltoid and upper trap involvement — requires good shoulder mobility and is best performed with moderate weight.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490096"))),
            Exercise.Create("Lu Raise", "A front-to-lateral raise hybrid where dumbbells are raised in front to shoulder height, then swept out laterally before lowering, training both the anterior and medial deltoid in one fluid motion.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490097"))),
            Exercise.Create("Prone Y Raise", "A face-down isolation exercise on an incline bench, raising the arms in a Y-shape overhead, targeting the lower traps, rear delts and rotator cuff — commonly used for shoulder health and posture correction.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490098"))),
            Exercise.Create("Dumbbell Reverse Fly", "A bent-over or chest-supported dumbbell fly with palms facing inward, isolating the posterior deltoid and rhomboids, performed with light weight and controlled tempo to maximise rear delt activation.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490099"))),

            // ——————————————————————————————————————————————
            // BACK — machine / dumbbell / cable / barbell variants
            // ——————————————————————————————————————————————
            Exercise.Create("Close Grip Lat Pulldown", "A lat pulldown using a narrow V-grip or close-grip handle, shifting emphasis toward the lower lats and increasing bicep involvement compared to wider grip variations.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490102"))),
            Exercise.Create("Straight Arm Pulldown", "A cable isolation exercise for the lats performed with locked elbows, pulling a bar or rope from overhead to the thighs, removing bicep involvement and targeting the lats through shoulder extension.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490103"))),
            Exercise.Create("Cable Pullover", "A standing lat isolation exercise using a high cable with a bar or rope, pulling downward in an arc with straight or slightly bent arms, providing constant tension through the full range of shoulder extension.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490104"))),
            Exercise.Create("Meadows Row", "A single-arm landmine row performed perpendicular to the barbell with a staggered stance, creating a unique arc of motion that emphasises the upper lats and teres major with a strong peak contraction.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490105"))),
            Exercise.Create("Pendlay Row", "A strict barbell row where the bar returns to the floor between each rep, eliminating momentum and stretch reflex, forcing concentric-only power from a dead stop — builds explosive pulling strength.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490106"))),
            Exercise.Create("Chin Up", "A vertical pull with a supinated (underhand) grip at shoulder width, increasing bicep recruitment compared to a pronated pull-up while still heavily loading the lats through a full range of motion.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490107"))),
            Exercise.Create("Inverted Row", "A bodyweight horizontal row performed under a low bar or suspension trainer, scaling difficulty by adjusting foot position — targets the mid-back, rear delts and biceps with no spinal compression.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490108"))),
            Exercise.Create("Rack Pull", "A partial-range deadlift starting from pins set at or above knee height, overloading the lockout portion of the pull and heavily targeting the upper back, traps and grip while reducing hamstring and lower-back demand.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490109"))),
            Exercise.Create("Dumbbell Pullover Row", "A two-in-one movement starting with a single-arm pullover from a bench, transitioning into a row at the top, training the lats through both shoulder extension and elbow flexion in one continuous motion.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490110"))),
            Exercise.Create("Neutral Grip Lat Pulldown", "A lat pulldown using parallel handles, placing the wrists in a neutral position that is often more comfortable for the shoulders and elbows while targeting the lats and lower traps.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490111"))),
            Exercise.Create("Wide Grip Seated Cable Row", "A seated cable row using a wide bar, pulling to the lower chest with elbows flared, shifting emphasis toward the rear deltoids, rhomboids and mid-traps compared to a close-grip row.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490112"))),
            Exercise.Create("Machine Lat Pulldown", "A plate-loaded or selectorised pulldown machine that provides a fixed arc of motion, making it easier to isolate the lats without grip or stabilisation being the limiting factor.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490113"))),
            Exercise.Create("Seal Row", "A strict chest-supported barbell row performed on an elevated bench, completely eliminating body English and lower-back involvement, forcing pure horizontal pulling from the upper and mid-back.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490114"))),

            // ——————————————————————————————————————————————
            // BICEPS — machine / dumbbell / cable / barbell variants
            // ——————————————————————————————————————————————
            Exercise.Create("Concentration Curl", "A seated single-arm curl with the elbow braced against the inner thigh, eliminating all momentum and shoulder involvement, isolating the biceps through a strict range of motion with a strong peak contraction.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490115"))),
            Exercise.Create("Spider Curl", "A curl performed prone on an incline bench with the arms hanging straight down, eliminating any possible cheat and placing the biceps in peak contraction at the top, with no rest point throughout the range.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490116"))),
            Exercise.Create("EZ Bar Curl", "A bicep curl using a cambered EZ bar that places the wrists in a semi-supinated position, reducing wrist strain compared to a straight barbell while effectively targeting the biceps brachii.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490117"))),
            Exercise.Create("Machine Bicep Curl", "A preacher or seated curl performed on a machine with a fixed arc, providing a consistent resistance curve and removing stabilisation demands — allows training close to failure safely.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490118"))),
            Exercise.Create("Reverse Curl", "A curl performed with a pronated (overhand) grip, shifting work away from the biceps and onto the brachioradialis and forearm extensors — builds the outer forearm and improves grip strength.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490121"))),
            Exercise.Create("Drag Curl", "A barbell curl where the elbows are driven backward rather than staying fixed, dragging the bar up the torso, which reduces front delt involvement and targets the long head of the biceps more aggressively.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490122"))),
            Exercise.Create("Cable Preacher Curl", "A preacher curl using a low cable instead of a barbell or dumbbell, providing constant tension through the full range including the top of the movement where free weights lose resistance.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490123"))),
            Exercise.Create("Zottman Curl", "A hybrid curl: supinated on the way up to maximise bicep activation, then pronated at the top and lowered with an overhand grip to eccentrically load the brachioradialis — trains both in one movement.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490124"))),

            // ——————————————————————————————————————————————
            // TRICEPS — machine / dumbbell / cable variants
            // ——————————————————————————————————————————————
            Exercise.Create("Cable Overhead Tricep Extension", "A tricep extension using a rope or bar attached to a low cable, performed facing away, placing the long head of the triceps under a deep stretch at the bottom — maximises long-head hypertrophy.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490125"))),
            Exercise.Create("Diamond Push Up", "A bodyweight push-up with hands placed close together forming a diamond shape, dramatically increasing tricep recruitment compared to standard push-ups while also working the inner chest.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490126"))),
            Exercise.Create("JM Press", "A hybrid between a close-grip bench press and a skull crusher, lowering the bar to the chin/neck area with elbows tucked, overloading the triceps with heavier weight than a standard skull crusher allows.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490127"))),
            Exercise.Create("Single Arm Cable Pushdown", "A unilateral tricep pushdown using one handle, allowing focus on each arm independently, correcting imbalances and enabling full pronation or neutral grip through the range of motion.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490128"))),
            Exercise.Create("Rope Pushdown", "A tricep pushdown using a rope attachment, allowing the hands to spread apart at the bottom for a stronger contraction of the lateral head, with a more natural wrist path than a straight bar.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490129"))),
            Exercise.Create("Machine Tricep Extension", "A seated tricep extension on a machine providing a fixed arc of motion, isolating the triceps without stability demands — particularly useful for training to failure safely without a spotter.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490130"))),
            Exercise.Create("Dumbbell Overhead Tricep Extension", "A single or two-arm overhead extension with a dumbbell, placing the long head of the triceps under maximal stretch at the bottom, which is key for long-head growth.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490131"))),
            Exercise.Create("Bench Dip", "A bodyweight tricep exercise with hands on a bench behind the body and feet on the floor, lowering the body by bending the elbows — difficulty scales by extending the legs or adding weight.", ExerciseType.Strength, ["Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490132"))),

            // ——————————————————————————————————————————————
            // QUADRICEPS — machine / barbell / dumbbell variants
            // ——————————————————————————————————————————————
            Exercise.Create("Front Squat", "A barbell squat with the bar racked on the front delts, forcing an upright torso position that shifts load toward the quadriceps and reduces shear force on the lower back compared to a back squat.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490133"))),
            Exercise.Create("Goblet Squat", "A squat holding a dumbbell or kettlebell at the chest, naturally encouraging an upright torso and proper depth — an excellent teaching tool for squat mechanics and a demanding quad builder at lighter loads.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490134"))),
            Exercise.Create("Sissy Squat", "A quad-isolation squat performed on the toes with the torso leaning backward, keeping the hips extended, placing extreme tension on the rectus femoris and patellar tendon — requires knee health and balance.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490135"))),
            Exercise.Create("Wall Sit", "An isometric lower-body hold with the back flat against a wall and knees bent at 90 degrees, building quadriceps endurance and time-under-tension without any spinal loading.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490136"))),
            Exercise.Create("Step Up", "A unilateral exercise stepping onto a raised platform (30-50 cm), targeting the quads and glutes of the leading leg while challenging balance and single-leg stability.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490137"))),
            Exercise.Create("Walking Lunge", "A dynamic lunge where each step travels forward continuously, combining quad and glute strength with hip flexor mobility and balance — can be loaded with dumbbells, barbell or bodyweight.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490138"))),
            Exercise.Create("Smith Machine Squat", "A squat performed on the Smith machine, which guides the bar along a vertical track, reducing balance demands and allowing focus on quad drive — foot position can be moved forward to increase quad bias.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490139"))),
            Exercise.Create("Pendulum Squat", "A machine squat with a pendulum lever arm that changes the resistance angle through the range, reducing load at the bottom (where joints are most vulnerable) and increasing it at the top.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490140"))),
            Exercise.Create("Belt Squat", "A squat loaded through a belt around the hips rather than on the shoulders, eliminating axial spinal loading entirely while still heavily targeting the quads and glutes — ideal for those with back issues.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490141"))),
            Exercise.Create("Reverse Lunge", "A lunge where the non-working leg steps backward, keeping the torso more upright and reducing knee shear on the front leg compared to a forward lunge — excellent for quad and glute development.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490142"))),
            Exercise.Create("Single Leg Leg Press", "A leg press performed one leg at a time, exposing and correcting bilateral strength imbalances while increasing range of motion per leg and glute activation.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490143"))),

            // ——————————————————————————————————————————————
            // HAMSTRINGS — machine / dumbbell / cable variants
            // ——————————————————————————————————————————————
            Exercise.Create("Seated Leg Curl", "A hamstring curl performed seated with the pad behind the ankles, emphasising the distal hamstrings and maintaining tension at the shortened position, which differs from the lying variant's stretch emphasis.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490144"))),
            Exercise.Create("Lying Leg Curl", "A prone hamstring curl on a flat or angled bench, providing the greatest stretch on the hamstrings at full extension and strongest contraction at full flexion — a staple hamstring isolator.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490145"))),
            Exercise.Create("Single Leg Romanian Deadlift", "A unilateral hip hinge performed on one leg while the other extends behind for balance, loading the hamstrings and glutes of the stance leg while demanding significant proprioception and ankle stability.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490146"))),
            Exercise.Create("Stiff Leg Deadlift", "A deadlift variation with minimal knee bend and the bar staying close to the legs, maximising the stretch on the hamstrings and loading the posterior chain through a large range of hip flexion.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490147"))),
            Exercise.Create("Dumbbell Romanian Deadlift", "A Romanian deadlift using dumbbells, allowing the weight to track along the sides of the legs, often reducing lower-back strain and allowing a freer hip hinge pattern than a barbell.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490148"))),
            Exercise.Create("Glute Ham Raise", "A posterior chain exercise performed on a GHD bench, starting with the torso horizontal and curling up using the hamstrings, combining knee flexion and hip extension in one demanding bodyweight movement.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490149"))),
            Exercise.Create("Cable Pull Through", "A hip hinge using a low cable between the legs, driving the hips forward against resistance, targeting the hamstrings and glutes with constant cable tension and minimal spinal loading.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490150"))),
            Exercise.Create("Kettlebell Swing", "A ballistic hip hinge swinging a kettlebell between the legs and driving it to chest height with an explosive hip snap, training the posterior chain (hamstrings, glutes, erectors) for power and endurance.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490151"))),

            // ——————————————————————————————————————————————
            // GLUTES — machine / cable / barbell variants
            // ——————————————————————————————————————————————
            Exercise.Create("Smith Machine Hip Thrust", "A hip thrust performed with a Smith machine bar across the hips, eliminating the need to balance the bar and allowing full focus on glute contraction and progressive overload.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490152"))),
            Exercise.Create("Glute Bridge", "A supine hip extension lying on the floor with feet flat, squeezing the glutes at the top — a lower-range-of-motion version of the hip thrust that is easier on the lower back.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490153"))),
            Exercise.Create("Single Leg Hip Thrust", "A unilateral hip thrust performed with one foot on the bench and the other elevated, doubling the load per glute and exposing side-to-side imbalances.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490154"))),
            Exercise.Create("Curtsy Lunge", "A lunge where the rear foot crosses behind the front leg in a curtsy motion, placing greater demand on the gluteus medius and hip abductors while also loading the quads.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490155"))),
            Exercise.Create("Frog Pump", "A glute bridge with the soles of the feet together and knees fallen outward, shortening the hip adductors and biasing the glutes as the primary hip extensor — high rep burnout exercise.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490156"))),
            Exercise.Create("Machine Glute Kickback", "A standing or kneeling kickback on a dedicated machine, extending the hip against resistance with a pad behind the ankle or knee, isolating the gluteus maximus through hip extension.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490157"))),
            Exercise.Create("Cable Hip Abduction", "A standing hip abduction using a low cable and ankle strap, lifting the leg laterally against resistance to target the gluteus medius and minimus — key for hip stability and knee health.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490158"))),

            // ——————————————————————————————————————————————
            // CALVES — machine / bodyweight variants
            // ——————————————————————————————————————————————
            Exercise.Create("Leg Press Calf Raise", "A calf raise performed on the leg press platform with the balls of the feet on the edge of the sled, allowing heavy loading of the gastrocnemius through a full range of plantar flexion.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490159"))),
            Exercise.Create("Smith Machine Calf Raise", "A standing calf raise using the Smith machine for balance and stability, allowing heavy bilateral loading of the gastrocnemius with a controlled range of motion.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490160"))),
            Exercise.Create("Donkey Calf Raise", "A calf raise performed bent at the hips with weight on the lower back or using a machine, placing the gastrocnemius in a pre-stretched position due to hip flexion, increasing the stretch at the bottom.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490161"))),
            Exercise.Create("Single Leg Calf Raise", "A unilateral calf raise performed on one leg, doubling the load per calf and exposing side-to-side strength or size imbalances — can be performed on a step for full range of motion.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490162"))),

            // ——————————————————————————————————————————————
            // CORE — cable / bodyweight / machine variants
            // ——————————————————————————————————————————————
            Exercise.Create("Ab Wheel Rollout", "A core anti-extension exercise using an ab wheel, rolling forward from the knees or standing while maintaining a braced neutral spine, creating extreme tension on the rectus abdominis and deep core stabilisers.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490163"))),
            Exercise.Create("Russian Twist", "A seated rotational core exercise holding a weight and twisting side to side with feet elevated, targeting the obliques and transverse abdominis — load should be light enough to maintain a neutral spine.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490164"))),
            Exercise.Create("Cable Woodchop", "A standing rotational exercise using a high or low cable, pulling diagonally across the body in a chopping motion, training the obliques and transverse abdominis through an anti-rotation and rotation pattern.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490165"))),
            Exercise.Create("Pallof Press", "An anti-rotation isometric exercise pressing a cable or band straight out from the chest while resisting the rotational pull, training the deep core stabilisers and obliques without spinal flexion.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490166"))),
            Exercise.Create("Side Plank", "A lateral isometric hold on one forearm, training the obliques, quadratus lumborum and hip abductors to resist lateral spinal flexion — a key exercise for frontal-plane core stability.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490167"))),
            Exercise.Create("Dead Bug", "A supine core exercise alternating opposite arm and leg extensions while maintaining a flat lower back against the floor, teaching core bracing and anti-extension in a low-impact, spine-safe position.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490168"))),
            Exercise.Create("Bird Dog", "A quadruped core exercise extending one arm forward and the opposite leg backward simultaneously, training anti-extension, anti-rotation and anti-lateral-flexion — a staple in spine rehab and warm-ups.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490169"))),
            Exercise.Create("Bicycle Crunch", "A dynamic crunch rotating the torso to bring elbow to opposite knee in a cycling motion, targeting the rectus abdominis and obliques with a rotational component — best performed slowly and controlled.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490170"))),
            Exercise.Create("Mountain Climber", "A dynamic plank-based exercise rapidly alternating knee drives toward the chest, combining core stabilisation with hip flexor work and cardiovascular demand at high tempos.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490171"))),
            Exercise.Create("Dragon Flag", "An advanced bodyweight core exercise lying on a bench, raising the entire body as a rigid lever from the shoulders, then lowering under control — demands extreme anterior core strength.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490172"))),
            Exercise.Create("L-Sit", "An isometric hold with the body supported on parallel bars or the floor, legs extended straight in front at hip height, demanding intense rectus abdominis, hip flexor and tricep engagement.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490173"))),
            Exercise.Create("Decline Sit Up", "A sit-up performed on a decline bench with the feet hooked, increasing the range of motion and gravitational resistance compared to a flat sit-up, heavily loading the hip flexors and upper abs.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490174"))),
            Exercise.Create("Lying Leg Raise", "A supine exercise raising straight or slightly bent legs from the floor to vertical, targeting the lower rectus abdominis and hip flexors — keeping the lower back pressed to the floor is critical.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490175"))),
            Exercise.Create("Toe Touch", "A supine crunch reaching the hands toward the toes with legs raised vertically, shortening the rectus abdominis through a full range of spinal flexion at the top.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490176"))),
            Exercise.Create("Copenhagen Plank", "A side plank variation with the top leg supported on a bench and the bottom leg hanging free, creating intense adductor engagement alongside oblique stabilisation — used in groin injury prevention.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490177"))),
            Exercise.Create("Suitcase Carry", "A unilateral loaded carry holding a heavy dumbbell or kettlebell in one hand while walking with an upright posture, demanding anti-lateral-flexion from the obliques and quadratus lumborum.", ExerciseType.Strength, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490178"))),

            // ——————————————————————————————————————————————
            // FOREARMS / GRIP — various variants
            // ——————————————————————————————————————————————
            Exercise.Create("Reverse Wrist Curl", "A wrist extension exercise with a pronated grip, targeting the forearm extensors (wrist extensors and brachioradialis) — important for elbow health and preventing lateral epicondylitis (tennis elbow).", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490179"))),
            Exercise.Create("Dead Hang", "A passive or active hang from a pull-up bar, stretching the lats and thoracic spine while building grip endurance and decompressing the spinal discs — timed sets build forearm endurance.", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490180"))),
            Exercise.Create("Plate Pinch", "A grip exercise holding two weight plates smooth-side-out between the thumb and fingers, training pinch grip strength that carries over to grappling, climbing and everyday object manipulation.", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490181"))),
            Exercise.Create("Fat Grip Curl", "Any curl variation performed with a thick bar or fat grip adapter, forcing the forearm flexors and extensors to work harder to maintain grip, building forearm size and crush grip strength simultaneously.", ExerciseType.Strength, ["Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490182"))),

            // ——————————————————————————————————————————————
            // TRAPS — dumbbell / cable / barbell variants
            // ——————————————————————————————————————————————
            Exercise.Create("Dumbbell Shrug", "A trap isolation using dumbbells at the sides, allowing a greater range of motion and more natural hand path than a barbell shrug, with the ability to retract the scapulae at the top.", ExerciseType.Strength, ["Traps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490183"))),
            Exercise.Create("Cable Shrug", "A shrug using a low cable or cable machine handles, providing constant tension through the full range including the stretched position at the bottom, unlike dumbbells which lose tension there.", ExerciseType.Strength, ["Traps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490184"))),
            Exercise.Create("Behind The Back Barbell Shrug", "A barbell shrug with the bar held behind the body (Smith machine or barbell), shifting emphasis slightly to the mid and lower traps and encouraging scapular retraction.", ExerciseType.Strength, ["Traps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490185"))),

            // ——————————————————————————————————————————————
            // OLYMPIC LIFTS & POWER MOVEMENTS
            // ——————————————————————————————————————————————
            Exercise.Create("Power Clean", "An explosive Olympic lift derivative pulling a barbell from the floor to the front-rack position in one motion, training triple extension (ankle, knee, hip) and full-body power production.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490186"))),
            Exercise.Create("Hang Clean", "A power clean starting from the hang position (above the knee), reducing the pull distance and emphasising the explosive hip drive and catch — more accessible than a full clean from the floor.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490187"))),
            Exercise.Create("Snatch", "A full Olympic lift pulling a barbell from the floor to overhead in one continuous motion with a wide grip, demanding extreme mobility, timing and power — the most technical barbell movement.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490188"))),
            Exercise.Create("Clean And Jerk", "A two-phase Olympic lift: cleaning the bar to the front rack, then jerking it overhead with a split or power jerk, testing maximal full-body strength and power.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490189"))),
            Exercise.Create("Push Press", "A standing overhead press initiated with a slight knee dip and leg drive, allowing heavier loads than a strict press while training the transfer of force from legs to arms.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490190"))),
            Exercise.Create("Hang Snatch", "A snatch starting from the hang position, simplifying the pull phase while still training the explosive hip extension and overhead catch with a wide grip.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490191"))),
            Exercise.Create("Muscle Up", "An advanced bodyweight skill combining a pull-up and dip in one continuous movement, transitioning from below the bar to above it — requires explosive pulling power and technique.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490192"))),
            Exercise.Create("Turkish Get Up", "A complex ground-to-standing movement holding a kettlebell or dumbbell locked out overhead throughout, training shoulder stability, core strength, hip mobility and full-body coordination.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490193"))),
            Exercise.Create("Thruster", "A front squat transitioning directly into an overhead press in one fluid motion, combining lower and upper body power — a staple in CrossFit and metabolic conditioning circuits.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490194"))),
            Exercise.Create("Man Maker", "A full-body complex: push-up, renegade row on each side, squat clean, and overhead press with dumbbells — one of the most metabolically demanding single movements.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490195"))),

            // ——————————————————————————————————————————————
            // MISCELLANEOUS STRENGTH
            // ——————————————————————————————————————————————
            Exercise.Create("Dumbbell Lateral Lunge", "A lateral step-out lunge holding dumbbells, loading the adductors, glutes and quads in the frontal plane — builds lateral stability and inner-thigh strength often neglected in linear training.", ExerciseType.Strength, ["Adductors"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490196"))),
            Exercise.Create("Cable Adduction", "A standing hip adduction with a low cable and ankle strap, pulling the leg inward across the body against resistance, isolating the adductor group — useful for groin injury prevention.", ExerciseType.Strength, ["Adductors"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490197"))),
            Exercise.Create("Renegade Row", "A plank-position dumbbell row alternating arms, combining anti-rotation core work with a single-arm row, demanding significant core stability alongside back and bicep strength.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490198"))),
            Exercise.Create("Trap Bar Deadlift", "A deadlift using a hexagonal trap bar, centering the load around the body rather than in front, reducing lumbar shear and allowing a more upright torso — often allows heavier loads than conventional.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490199"))),
            Exercise.Create("Deficit Deadlift", "A conventional deadlift standing on a raised platform (2-4 inches), increasing the range of motion at the bottom and building strength off the floor — targets the quads and erectors harder.", ExerciseType.Strength, ["Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490200"))),
            Exercise.Create("Pause Squat", "A squat with a deliberate 2-3 second pause at the bottom, eliminating the stretch reflex and building concentric strength, positional awareness and confidence in the hole.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490201"))),
            Exercise.Create("Safety Bar Squat", "A squat using a safety squat bar with front-loaded handles and a cambered yoke, reducing shoulder stress and shifting the centre of gravity forward, increasing upper-back and quad demand.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490202"))),
            Exercise.Create("Zercher Squat", "A squat holding the barbell in the crooks of the elbows, forcing an extremely upright torso and demanding intense core bracing, anterior loading and bicep endurance.", ExerciseType.Strength, ["Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490203"))),
            Exercise.Create("Nordic Hip Hinge", "A knee-dominant eccentric exercise on a GHD or with feet anchored, slowly lowering the torso forward from the knees with the hips locked — extreme hamstring eccentric loading for injury prevention.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490204"))),
            Exercise.Create("Cable Face Pull With External Rotation", "A face pull that continues into an external rotation at the top, training the rear delts, lower traps and rotator cuff external rotators in one movement — a premier shoulder health exercise.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490205"))),
            Exercise.Create("Band Pull Apart", "A resistance band exercise pulling the band apart horizontally at shoulder height, targeting the rear delts, rhomboids and mid-traps — commonly used as a warm-up or high-rep postural corrective.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490206"))),
            Exercise.Create("Chest Supported Dumbbell Lateral Raise", "A lateral raise lying face-down on an incline bench, preventing any swinging or momentum, isolating the medial deltoid with strict form at lighter weights.", ExerciseType.Strength, ["Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490207"))),
            Exercise.Create("Machine Leg Curl", "A general machine-based hamstring curl (lying or seated) with adjustable resistance, providing smooth resistance and easy drop-set capability for hamstring hypertrophy.", ExerciseType.Strength, ["Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490208"))),
            Exercise.Create("Machine Calf Raise", "A seated or standing calf raise machine providing plate-loaded or selectorised resistance, allowing precise progressive overload on the gastrocnemius or soleus depending on knee position.", ExerciseType.Strength, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490209"))),
            Exercise.Create("Overhead Squat", "A squat performed with a barbell locked out overhead in a wide snatch grip, demanding extreme shoulder and thoracic mobility, core stability and ankle flexibility — a key Olympic lifting accessory.", ExerciseType.Strength, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490210"))),

            // ——————————————————————————————————————————————
            // CARDIO — comprehensive (Strava / Garmin inspired)
            // ——————————————————————————————————————————————
            Exercise.Create("Outdoor Run", "Running outdoors on roads, paths or tracks — the most natural form of cardiovascular exercise, loading bones and connective tissue more than a treadmill due to varied terrain and wind resistance.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490211"))),
            Exercise.Create("Trail Run", "Off-road running on unpaved trails, incorporating elevation change, uneven surfaces and technical terrain, demanding greater ankle stability, proprioception and mental focus than road running.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490212"))),
            Exercise.Create("Walking", "A low-intensity steady-state cardiovascular activity that improves heart health, aids recovery, and contributes significantly to daily energy expenditure (NEAT) with minimal joint stress.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490213"))),
            Exercise.Create("Hiking", "Extended walking on trails with elevation gain, often with a loaded pack, combining cardiovascular endurance with lower-body strength — duration and terrain make it more demanding than casual walking.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490214"))),
            Exercise.Create("Road Cycling", "Cycling on paved roads using a road or endurance bike, primarily training aerobic capacity and quad/glute endurance with low joint impact — cadence and gearing control intensity.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490215"))),
            Exercise.Create("Mountain Biking", "Off-road cycling on technical trails with climbs and descents, combining cardiovascular endurance with upper-body shock absorption, core stabilisation and bike handling skills.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490216"))),
            Exercise.Create("Gravel Cycling", "Cycling on unpaved roads and gravel paths, bridging road and mountain biking — longer efforts on varied surfaces that demand endurance and upper-body vibration dampening.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490217"))),
            Exercise.Create("Indoor Cycling", "A structured cycling session on a stationary or smart trainer, often in a spin class or using virtual platforms like Zwift — allows precise wattage control for interval and endurance training.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490218"))),
            Exercise.Create("E-Bike Ride", "Cycling with electric pedal assistance, reducing intensity on climbs and allowing longer distances — still provides cardiovascular benefit, especially on hilly terrain where sustained effort is required.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490219"))),
            Exercise.Create("Sprint", "Short maximal-effort running bouts (50-400m), training the anaerobic energy system, fast-twitch muscle fibres and neuromuscular power — rest intervals between efforts are critical for quality.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490220"))),
            Exercise.Create("Interval Run", "A structured run alternating between hard efforts and recovery jogs — examples include 400m repeats, tempo intervals or fartlek sessions, training VO2max and lactate threshold.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490221"))),
            Exercise.Create("Track Workout", "A running session on an athletics track, typically involving measured intervals (200m, 400m, 800m, mile repeats) with timed recovery, used for speed development and race preparation.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490222"))),
            Exercise.Create("Cross Country Skiing", "A winter endurance sport using poles and skis to traverse snow-covered terrain, engaging the entire body — one of the most demanding cardiovascular activities, training upper and lower body simultaneously.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Glutes", "Core", "Shoulders", "Triceps", "Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490223"))),
            Exercise.Create("Snowshoeing", "Walking or running on snow with snowshoes, significantly increasing energy expenditure compared to regular walking due to the added weight and resistance of breaking trail in deep snow.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490224"))),
            Exercise.Create("Outdoor Snowboarding", "Descending snow-covered slopes on a snowboard, demanding isometric quad and glute endurance for edge control, core rotation for turns, and anaerobic fitness for repeated runs.", ExerciseType.Cardio, ["Quadriceps", "Core", "Glutes", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490225"))),
            Exercise.Create("Skiing", "Downhill or alpine skiing, requiring sustained isometric quad contraction for carving, core engagement for balance, and hip abduction for edge transitions — highly demanding on the lower body.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Glutes", "Core", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490226"))),
            Exercise.Create("Ski Touring", "Ascending ski slopes under own power using climbing skins, then skiing down — combines the cardiovascular demand of cross-country skiing with the technical descents of alpine skiing.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Glutes", "Core", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490227"))),
            Exercise.Create("Kayaking", "Paddling a kayak using a double-bladed paddle, primarily training the lats, shoulders and core rotators while providing a sustained upper-body cardiovascular workout on water.", ExerciseType.Cardio, ["Back", "Shoulders", "Core", "Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490228"))),
            Exercise.Create("Canoeing", "Paddling a canoe with a single-bladed paddle, alternating sides, training rotational core strength, lat endurance and shoulder stability — more asymmetric loading than kayaking.", ExerciseType.Cardio, ["Back", "Shoulders", "Core", "Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490229"))),
            Exercise.Create("Stand Up Paddleboarding", "Paddling a wide board while standing, combining core balance with upper-body paddling endurance — even calm conditions demand constant ankle and core micro-adjustments for balance.", ExerciseType.Cardio, ["Core", "Shoulders", "Back", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490230"))),
            Exercise.Create("Surfing", "Wave riding on a surfboard, combining explosive pop-ups (similar to a burpee), sustained paddling (shoulder endurance), and dynamic balance during rides — highly variable intensity.", ExerciseType.Cardio, ["Shoulders", "Core", "Back", "Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490231"))),
            Exercise.Create("Open Water Swimming", "Swimming in lakes, rivers or the ocean without lane ropes or walls, demanding continuous effort without turns, plus sighting navigation and adapting to currents and chop.", ExerciseType.Cardio, ["Back", "Shoulders", "Core", "Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490232"))),
            Exercise.Create("Inline Skating", "Skating on inline (roller) skates on roads or paths, providing a low-impact cardiovascular workout that heavily loads the quads, glutes and hip abductors through a lateral push-off stride.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Calves", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490233"))),
            Exercise.Create("Ice Skating", "Skating on ice rinks or frozen surfaces, combining cardiovascular endurance with strong quad and glute engagement through the lateral push-off and single-leg balance phases.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Calves", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490234"))),
            Exercise.Create("Skateboarding", "Board sport on paved surfaces involving pushing, balancing and trick execution, training single-leg balance, ankle stability and intermittent cardiovascular effort.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490235"))),
            Exercise.Create("Tennis", "A racquet sport combining short explosive sprints, lateral shuffles, overhead reaching and rotational power — demands agility, hand-eye coordination and repeated anaerobic bursts with aerobic recovery.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Shoulders", "Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490236"))),
            Exercise.Create("Padel", "A racquet sport played in a glass-walled court with a solid paddle, combining tennis-like movements with wall play — lower impact than tennis but equally demanding in lateral agility and reflexes.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490237"))),
            Exercise.Create("Badminton", "A racquet sport using a shuttlecock, requiring explosive lunges, overhead smashes and rapid directional changes — deceptively demanding on the legs, core and racquet-arm shoulder.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490238"))),
            Exercise.Create("Squash", "A high-intensity racquet sport played in a four-walled court, involving constant movement, lunging and twisting — one of the most calorically demanding racquet sports per minute of play.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Shoulders", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490239"))),
            Exercise.Create("Table Tennis", "A paddle sport demanding rapid reflexes, wrist speed and footwork at moderate cardiovascular intensity — improves hand-eye coordination and reaction time.", ExerciseType.Cardio, ["Forearms", "Core", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490240"))),
            Exercise.Create("Pickleball", "A paddle sport on a smaller court with a perforated ball, combining elements of tennis, badminton and ping pong — moderate intensity with frequent lateral movement and overhead play.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490241"))),
            Exercise.Create("Soccer", "A field sport involving sustained running, sprinting, cutting and kicking over 90 minutes, demanding high aerobic capacity, anaerobic power, agility and lower-body coordination.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490242"))),
            Exercise.Create("Basketball", "A court sport with constant running, jumping, lateral shuffling and explosive acceleration, demanding both aerobic endurance and anaerobic power alongside coordination and vertical leap.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Glutes", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490243"))),
            Exercise.Create("Volleyball", "A court sport involving explosive jumps, lateral movement, diving and overhead hitting — trains vertical power, shoulder endurance and anaerobic fitness in short intense rallies.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Shoulders", "Core", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490244"))),
            Exercise.Create("Handball", "A fast-paced team sport combining running, jumping, throwing and physical contact, demanding aerobic endurance, upper-body power and agility over two 30-minute halves.", ExerciseType.Cardio, ["Quadriceps", "Shoulders", "Core", "Glutes", "Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490245"))),
            Exercise.Create("Rugby", "A full-contact field sport involving sprinting, tackling, scrummaging and sustained running, demanding high aerobic and anaerobic fitness alongside upper and lower body strength.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490246"))),
            Exercise.Create("Ice Hockey", "A high-speed sport on ice combining explosive skating, stick handling, shooting and body contact — demands anaerobic power, core stability and hip/groin flexibility.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Core", "Shoulders", "Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490247"))),
            Exercise.Create("Field Hockey", "A running-intensive sport on turf involving a low body position for stick work, sprinting and rapid direction changes — heavily loads the quads, hamstrings and lower back.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Core", "Glutes", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490248"))),
            Exercise.Create("Cricket", "A bat-and-ball sport involving intermittent sprinting, throwing, bowling and batting, with cardiovascular demand varying by position — fast bowlers have the highest physical load.", ExerciseType.Cardio, ["Quadriceps", "Shoulders", "Core", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490249"))),
            Exercise.Create("Boxing", "A combat sport involving footwork, punching combinations and defensive movement, training upper-body endurance, core rotation, hand speed and intense anaerobic and aerobic conditioning.", ExerciseType.Cardio, ["Shoulders", "Core", "Calves", "Back", "Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490250"))),
            Exercise.Create("Kickboxing", "A striking martial art combining punches with kicks, training full-body power, flexibility, balance and cardiovascular endurance — more lower-body demanding than boxing.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Core", "Shoulders", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490251"))),
            Exercise.Create("Martial Arts", "A general training session in any discipline (karate, judo, taekwondo, BJJ, MMA etc.), combining technique drills, sparring and conditioning — full-body workout with variable intensity.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490252"))),
            Exercise.Create("Wrestling", "A grappling sport demanding explosive strength, sustained muscular endurance, flexibility and anaerobic capacity — one of the most physically taxing combat sports per minute.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490253"))),
            Exercise.Create("Brazilian Jiu Jitsu", "A ground-based grappling art involving positional control and submissions, training grip endurance, hip mobility, core strength and isometric muscular endurance.", ExerciseType.Cardio, ["Core", "Back", "Shoulders", "Forearms", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490254"))),
            Exercise.Create("Yoga", "A practice combining held postures, transitions and breathwork, developing flexibility, isometric strength, balance and body awareness — intensity varies from restorative to power yoga.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490255"))),
            Exercise.Create("Pilates", "A controlled movement practice emphasising core stability, spinal alignment, breathing and mind-body connection, using bodyweight or reformer machines for low-impact full-body conditioning.", ExerciseType.Cardio, ["Core", "Glutes", "Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490256"))),
            Exercise.Create("Dance", "A choreographed or freestyle movement session (salsa, hip hop, contemporary, ballroom etc.), combining cardiovascular endurance with coordination, rhythm, flexibility and expression.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490257"))),
            Exercise.Create("HIIT", "A structured session of high-intensity interval training alternating all-out effort (85-100% max HR) with rest or active recovery, maximising caloric burn, VO2max and EPOC in a short time.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490258"))),
            Exercise.Create("CrossFit WOD", "A CrossFit Workout of the Day: a timed or scored mixed-modal session combining weightlifting, gymnastics and metabolic conditioning — intensity is high and movements vary daily.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490259"))),
            Exercise.Create("Stretching", "A dedicated flexibility session using static, dynamic or PNF stretching techniques to improve joint range of motion, reduce muscle tension and support recovery from training.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490260"))),
            Exercise.Create("Foam Rolling", "A self-myofascial release session using a foam roller to apply pressure to tight muscles, improving blood flow, reducing perceived soreness and restoring tissue extensibility.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490261"))),
            Exercise.Create("Sled Pull", "A functional exercise pulling a weighted sled toward the body using a rope or harness, training the posterior chain (hamstrings, glutes, back) and grip in a concentric-only pattern.", ExerciseType.Cardio, ["Hamstrings", "Glutes", "Back", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490262"))),
            Exercise.Create("Ski Erg", "A standing pull-down machine simulating Nordic skiing, primarily training the lats, triceps and core while delivering intense cardiovascular conditioning with minimal lower-body impact.", ExerciseType.Cardio, ["Back", "Triceps", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490263"))),
            Exercise.Create("Versa Climber", "A vertical climbing machine simulating ladder climbing, engaging the full body with simultaneous arm and leg movement — extremely high caloric demand per minute with low joint impact.", ExerciseType.Cardio, ["Quadriceps", "Glutes", "Shoulders", "Core", "Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490264"))),
            Exercise.Create("Rucking", "Walking with a weighted backpack (typically 10-30 kg), significantly increasing cardiovascular and muscular demand over regular walking while building postural endurance and mental resilience.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490265"))),
            Exercise.Create("Aqua Jogging", "Running in deep water using a flotation belt, eliminating all impact while maintaining a running-specific cardiovascular stimulus — a key injury-rehabilitation and cross-training modality.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Core", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490266"))),
            Exercise.Create("Triathlon Training", "A combined swim-bike-run session or brick workout (two disciplines back-to-back), training the body to transition between movement patterns and energy demands — sport-specific endurance.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490267"))),
            Exercise.Create("Golf", "A walking sport covering 6-10 km per round with rotational club swings, training core rotation, hip mobility and moderate cardiovascular endurance — walking rounds increase the fitness benefit.", ExerciseType.Cardio, ["Core", "Shoulders", "Glutes", "Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490268"))),
            Exercise.Create("Fencing", "A combat sport involving explosive lunges, retreats and lateral movement with a weapon, training lower-body power, reaction time and anaerobic fitness in short intense bouts.", ExerciseType.Cardio, ["Quadriceps", "Calves", "Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490269"))),
            Exercise.Create("Climbing", "Outdoor rock climbing on natural walls, combining grip strength, upper-body pulling power, hip flexibility and problem-solving — sustained effort makes it both strength and cardiovascular training.", ExerciseType.Cardio, ["Forearms", "Back", "Core", "Shoulders", "Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490270"))),
            Exercise.Create("Rowing (On Water)", "Rowing a boat on open water using oars, engaging the legs (drive), core (stability) and back/arms (pull) in a coordinated full-body stroke — a classic endurance sport.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Back", "Core", "Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490271"))),
            Exercise.Create("Snowmobiling", "Operating a snowmobile over varied terrain, engaging the core and upper body for steering and balance — surprisingly physical in deep snow and technical conditions.", ExerciseType.Cardio, ["Core", "Shoulders", "Forearms"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490272"))),
            Exercise.Create("Horseback Riding", "Equestrian activity requiring sustained core engagement, hip mobility and inner-thigh isometric contraction to maintain balance and communicate with the horse through body position.", ExerciseType.Cardio, ["Core", "Adductors", "Glutes", "Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490273"))),
            Exercise.Create("Sailing", "Operating a sailboat, involving sustained pulling, hiking out, winching and balance — physically demanding in heavy wind and waves, training grip, core and full-body endurance.", ExerciseType.Cardio, ["Core", "Forearms", "Back", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490274"))),
            Exercise.Create("Wheelchair Racing", "Sprint or distance racing in a racing wheelchair, demanding extreme upper-body power and endurance, shoulder health and trunk stability — a Paralympic-level cardiovascular discipline.", ExerciseType.Cardio, ["Shoulders", "Back", "Triceps", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490275"))),
            Exercise.Create("Hand Cycling", "Cycling using arm-powered cranks, providing an upper-body cardiovascular workout suitable for those with lower-limb limitations or as a cross-training modality for able-bodied athletes.", ExerciseType.Cardio, ["Shoulders", "Triceps", "Back", "Core", "Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490276"))),
            Exercise.Create("Nordic Walking", "Walking with specially designed poles, engaging the upper body in each stride to increase energy expenditure by 20-40% over regular walking while reducing lower-body joint stress.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Shoulders", "Triceps", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490277"))),
            Exercise.Create("Waterpolo", "A team sport played in deep water involving treading water, swimming sprints, throwing and physical contact — extremely demanding on aerobic and anaerobic systems simultaneously.", ExerciseType.Cardio, ["Shoulders", "Core", "Quadriceps", "Back", "Triceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490278"))),
            Exercise.Create("Lacrosse", "A field sport involving running, cradling, passing and shooting with a stick, combining sustained running with explosive sprints and upper-body coordination.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Shoulders", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490279"))),
            Exercise.Create("Ultimate Frisbee", "A running-intensive team sport involving sprinting, cutting, jumping and throwing a disc — demands high aerobic capacity and repeated-sprint ability.", ExerciseType.Cardio, ["Quadriceps", "Hamstrings", "Calves", "Glutes", "Shoulders", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490280"))),

            // ——————————————————————————————————————————————
            // PLYOMETRIC — expanded
            // ——————————————————————————————————————————————
            Exercise.Create("Squat Jump", "A bodyweight squat followed by an explosive vertical jump, training lower-body power production through the stretch-shortening cycle — a foundational plyometric for all athletes.", ExerciseType.Plyometric, ["Quadriceps", "Glutes", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490281"))),
            Exercise.Create("Split Jump", "A lunge-position jump alternating legs in mid-air, training unilateral explosive power, hip flexor mobility and single-leg landing mechanics — also called jump lunges or scissor jumps.", ExerciseType.Plyometric, ["Quadriceps", "Glutes", "Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490282"))),
            Exercise.Create("Skater Jump", "A lateral plyometric bounding from one leg to the other, mimicking a speed skater's stride, training hip abductors, glutes and single-leg landing stability in the frontal plane.", ExerciseType.Plyometric, ["Glutes", "Quadriceps", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490283"))),
            Exercise.Create("Drop Jump", "Stepping off a box and immediately rebounding upon ground contact with minimal ground contact time, training the stretch-shortening cycle's reactive strength — a true reactive plyometric.", ExerciseType.Plyometric, ["Quadriceps", "Calves", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490284"))),
            Exercise.Create("Ankle Hop", "A continuous low-amplitude bouncing on the balls of the feet with stiff ankles and minimal knee bend, training calf and Achilles tendon stiffness for improved running economy.", ExerciseType.Plyometric, ["Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490285"))),
            Exercise.Create("Pogo Jump", "A rhythmic vertical jump with stiff legs, emphasising rapid ground contact and ankle/calf power — trains tendon elasticity and reactive strength for sprint and jump performance.", ExerciseType.Plyometric, ["Calves", "Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490286"))),
            Exercise.Create("Reactive Step Up", "An explosive single-leg step-up onto a box immediately driving the knee up and switching legs upon descent, training unilateral power and rapid ground contact.", ExerciseType.Plyometric, ["Quadriceps", "Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490287"))),
            Exercise.Create("Lateral Box Jump", "A box jump performed laterally, jumping sideways onto the box, training frontal-plane power and hip abductor force production — important for court and field sports.", ExerciseType.Plyometric, ["Glutes", "Quadriceps", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490288"))),
            Exercise.Create("Medicine Ball Chest Pass", "An explosive two-handed push of a medicine ball from the chest against a wall or to a partner, training upper-body horizontal pushing power and chest/tricep rate of force development.", ExerciseType.Plyometric, ["Chest", "Triceps", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490289"))),
            Exercise.Create("Medicine Ball Overhead Throw", "An explosive overhead throw of a medicine ball, extending from full hip flexion through triple extension, training posterior chain power and overhead force production.", ExerciseType.Plyometric, ["Core", "Shoulders", "Back"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490290"))),
            Exercise.Create("Medicine Ball Rotational Throw", "A standing rotational throw of a medicine ball against a wall, training oblique power, hip rotation speed and core force transfer — mimics throwing, batting and racquet sport mechanics.", ExerciseType.Plyometric, ["Core", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490291"))),
            Exercise.Create("Bounding", "An exaggerated running stride with maximal flight time between each ground contact, training hip extension power, hamstring-glute coordination and horizontal force production.", ExerciseType.Plyometric, ["Glutes", "Hamstrings", "Calves", "Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490292"))),
            Exercise.Create("Wall Ball", "A squat-to-overhead throw with a medicine ball against a high wall target, combining a front squat with an explosive push press — a metabolic plyometric staple in CrossFit programming.", ExerciseType.Plyometric, ["Quadriceps", "Glutes", "Shoulders", "Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490293"))),
            Exercise.Create("Plyo Push Up", "A push-up where the hands leave the ground at the top of each rep, training upper-body reactive strength and chest/tricep rate of force development — progress from kneeling to full to airborne.", ExerciseType.Plyometric, ["Chest", "Triceps", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490294"))),
            Exercise.Create("Weighted Box Jump", "A box jump performed holding dumbbells or wearing a weighted vest, increasing the load on the stretch-shortening cycle and demanding greater concentric power output to clear the box.", ExerciseType.Plyometric, ["Quadriceps", "Glutes", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490295"))),
            Exercise.Create("Single Leg Box Jump", "A box jump performed from one leg, demanding significantly more unilateral power, balance and landing control — an advanced plyometric for single-leg sport demands.", ExerciseType.Plyometric, ["Quadriceps", "Glutes", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490296"))),
            Exercise.Create("Rotational Jump", "A vertical jump with a 90-180 degree rotation in the air, training vestibular awareness, rotational power and landing mechanics under rotational forces.", ExerciseType.Plyometric, ["Quadriceps", "Core", "Glutes", "Calves"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490297"))),
            Exercise.Create("Alternating Lunge Jump", "A continuous plyometric lunge alternating legs with each jump, training unilateral power endurance, hip flexor mobility and cardiovascular capacity at high intensity.", ExerciseType.Plyometric, ["Quadriceps", "Glutes", "Hamstrings"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490298"))),
            Exercise.Create("Star Jump", "A jumping jack variant reaching full extension in an X-shape at the peak, training total-body explosive power and coordination — can be performed from a squat for added intensity.", ExerciseType.Plyometric, ["Quadriceps", "Glutes", "Calves", "Shoulders"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490299"))),
            Exercise.Create("Sprint Bound", "A maximal-effort bounding drill performed at sprint speeds, combining horizontal velocity with exaggerated flight phases, training the hip extensors and elastic energy return of tendons.", ExerciseType.Plyometric, ["Glutes", "Hamstrings", "Calves", "Quadriceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490300"))),

        };

        var toAdd = allExercises.Where(e => !existingSet.Contains(e.Id.Value)).ToList();
        if (toAdd.Count > 0)
        {
            context.Exercises.AddRange(toAdd);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedTemplates()
    {
        if (await context.WorkoutTemplates.AnyAsync())
            return;

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
        var treadmillRun    = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490051"));
        var rowingMachine   = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490053"));

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

            WorkoutTemplate.Create(SeededUserId, "Cardio & Strength", [
                TemplateExercise.Create(treadmillRun, 1, [
                    PlannedSet.CreateCardio(1, TimeSpan.FromMinutes(10), 2m, PlannedDistanceUnit.Km),
                    PlannedSet.CreateCardio(2, TimeSpan.FromMinutes(10), 2m, PlannedDistanceUnit.Km),
                ]),
                TemplateExercise.Create(benchPress, 2, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 10),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 10),
                ]),
                TemplateExercise.Create(rowingMachine, 3, [
                    PlannedSet.CreateCardio(1, TimeSpan.FromMinutes(5), 1m, PlannedDistanceUnit.Km),
                    PlannedSet.CreateCardio(2, TimeSpan.FromMinutes(5), 1m, PlannedDistanceUnit.Km),
                    PlannedSet.CreateCardio(3, TimeSpan.FromMinutes(5), 1m, PlannedDistanceUnit.Km),
                ]),
                TemplateExercise.Create(latPulldown, 4, [
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 1, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 2, 12),
                    PlannedSet.CreateStrengthLike(ExerciseType.Strength, 3, 12),
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

        // Exercise IDs
        var benchPress          = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490001"));
        var overheadPress       = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490006"));
        var lateralRaise        = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490007"));
        var reverseFlye         = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490009"));
        var latPulldown         = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490013"));
        var seatedCableRow      = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490014"));
        var dumbbellCurl        = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490017"));
        var hammerCurl          = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490018"));
        var preacherCurl        = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490019"));
        var cableCurl           = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490020"));
        var tricepPushdown      = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490021"));
        var overheadTricep      = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490023"));
        var squat               = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490026"));
        var legPress            = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490027"));
        var legExtension        = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490028"));
        var hackSquat           = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490030"));
        var legCurl             = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490032"));
        var calfRaise           = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490037"));
        var wristCurl           = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490043"));
        var facePull            = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490045"));
        var pullUp              = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490046"));
        var dip                 = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490047"));
        var inclineDumbbellCurl = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490050"));
        var treadmill           = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490051"));
        var hipAdductor         = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490071"));
        var hipAbductor         = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490072"));
        var sitUp               = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490073"));
        var chestFly            = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490074"));
        var bouldering          = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490075"));
        var indoorSnowboarding  = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490076"));
        var inclineDumbbellPress = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490077"));
        var machineShoulderPress = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490078"));
        var chestPress          = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490079"));
        var machineRow          = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490080"));
        var tbarRow             = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490081"));
        var chestSupportedRow   = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490082"));
        var cableHammerCurl     = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490083"));
        var forearmCurl         = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490084"));
        var calfPress           = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490085"));
        var machineCrunch       = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490086"));
        var crunch              = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490087"));
        var bayesianCurl        = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490088"));
        var reverseForearmCurl  = EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490089"));

        static LoggedSet S(int order, int reps, decimal weight, int rir = 2)
            => LoggedSet.CreateStrength(order, reps, weight, WeightUnit.Kg, rir);

        var logs = new List<LoggedWorkout>
        {
            // 02/01/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legExtension, 1, [S(1,8,73), S(2,8,86), S(3,8,93)]),
                LoggedExercise.Create(legCurl, 2, [S(1,8,52), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(hipAdductor, 3, [S(1,12,45), S(2,12,52)]),
                LoggedExercise.Create(hipAbductor, 4, [S(1,12,45), S(2,12,52)]),
                LoggedExercise.Create(machineShoulderPress, 5, [S(1,10,23), S(2,10,27), S(3,10,23)]),
            ], loggedAt: new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)),

            // 03/01/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(inclineDumbbellPress, 1, [S(1,8,24), S(2,8,26), S(3,8,26)]),
                LoggedExercise.Create(lateralRaise, 2, [S(1,8,6.8m), S(2,8,6.8m)]),
                LoggedExercise.Create(tricepPushdown, 3, [S(1,10,22), S(2,10,24), S(3,10,22)]),
                LoggedExercise.Create(overheadTricep, 4, [S(1,6,15), S(2,6,13)]),
                LoggedExercise.Create(chestFly, 5, [S(1,8,86), S(2,8,86), S(3,8,100)]),
            ], loggedAt: new DateTime(2026, 1, 3, 12, 0, 0, DateTimeKind.Utc)),

            // 05/01/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(pullUp, 1, [S(1,6,0,0), S(2,6,0,0)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(dumbbellCurl, 3, [S(1,6,16), S(2,6,12), S(3,6,12), S(4,6,14)]),
                LoggedExercise.Create(latPulldown, 4, [S(1,8,59), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(wristCurl, 5, [S(1,8,27), S(2,8,27)]),
            ], loggedAt: new DateTime(2026, 1, 5, 12, 0, 0, DateTimeKind.Utc)),

            // 08/01/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(squat, 1, [S(1,8,60), S(2,8,60), S(3,8,70)]),
                LoggedExercise.Create(legPress, 2, [S(1,10,155), S(2,10,195)]),
                LoggedExercise.Create(legCurl, 3, [S(1,10,52), S(2,10,59)]),
                LoggedExercise.Create(legExtension, 4, [S(1,8,73), S(2,8,86), S(3,8,93)]),
                LoggedExercise.Create(hipAdductor, 5, [S(1,10,52), S(2,10,52)]),
                LoggedExercise.Create(hipAbductor, 6, [S(1,10,52), S(2,10,52)]),
            ], loggedAt: new DateTime(2026, 1, 8, 12, 0, 0, DateTimeKind.Utc)),

            // 09/01/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(chestPress, 1, [S(1,8,45), S(2,8,52), S(3,8,59)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,8,26), S(2,8,24)]),
                LoggedExercise.Create(machineShoulderPress, 3, [S(1,6,27), S(2,6,32), S(3,6,35)]),
                LoggedExercise.Create(chestFly, 4, [S(1,6,92), S(2,6,100), S(3,6,100)]),
                LoggedExercise.Create(overheadTricep, 5, [S(1,8,15), S(2,8,18)]),
                LoggedExercise.Create(tricepPushdown, 6, [S(1,6,20), S(2,6,22), S(3,6,20)]),
                LoggedExercise.Create(lateralRaise, 7, [S(1,8,6.8m), S(2,8,6.8m)]),
            ], loggedAt: new DateTime(2026, 1, 9, 12, 0, 0, DateTimeKind.Utc)),

            // 12/01/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(pullUp, 1, [S(1,10,0,0), S(2,10,0,0), S(3,10,0,0)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,8,59), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(preacherCurl, 3, [S(1,8,27.5m), S(2,8,27.5m), S(3,8,27.5m)]),
                LoggedExercise.Create(wristCurl, 4, [S(1,8,22), S(2,8,27), S(3,8,29)]),
                LoggedExercise.Create(seatedCableRow, 5, [S(1,8,59), S(2,8,66)]),
            ], loggedAt: new DateTime(2026, 1, 12, 12, 0, 0, DateTimeKind.Utc)),

            // 14/01/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(squat, 1, [S(1,6,60), S(2,6,60), S(3,6,80)]),
                LoggedExercise.Create(legPress, 2, [S(1,8,0,0), S(2,8,0,0), S(3,8,0,0)]),
                LoggedExercise.Create(legExtension, 3, [S(1,8,0,0), S(2,8,0,0), S(3,8,0,0)]),
                LoggedExercise.Create(legCurl, 4, [S(1,8,0,0), S(2,8,0,0)]),
                LoggedExercise.Create(hipAdductor, 5, [S(1,10,0,0), S(2,10,0,0)]),
                LoggedExercise.Create(hipAbductor, 6, [S(1,10,0,0), S(2,10,0,0)]),
                LoggedExercise.Create(calfRaise, 7, [S(1,8,0,0), S(2,8,0,0), S(3,8,0,0)]),
            ], loggedAt: new DateTime(2026, 1, 14, 12, 0, 0, DateTimeKind.Utc)),

            // 18/01/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,6,70), S(2,6,70), S(3,6,70)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,6,26), S(2,6,26), S(3,6,28)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,6,24), S(2,6,20)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,8,6.8m), S(2,8,4.8m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,22), S(2,8,20), S(3,8,22)]),
                LoggedExercise.Create(chestFly, 6, [S(1,8,86), S(2,8,93), S(3,8,100), S(4,8,100)]),
            ], loggedAt: new DateTime(2026, 1, 18, 12, 0, 0, DateTimeKind.Utc)),

            // 20/01/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(latPulldown, 1, [S(1,8,59), S(2,8,66), S(3,8,59)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,6,66), S(2,6,66), S(3,6,66)]),
                LoggedExercise.Create(preacherCurl, 3, [S(1,6,27.5m), S(2,6,27.5m), S(3,6,27.5m)]),
                LoggedExercise.Create(cableHammerCurl, 4, [S(1,8,13), S(2,8,18)]),
                LoggedExercise.Create(reverseFlye, 5, [S(1,12,6.8m), S(2,12,4.2m)]),
            ], loggedAt: new DateTime(2026, 1, 20, 12, 0, 0, DateTimeKind.Utc)),

            // 21/01/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legPress, 1, [S(1,8,195), S(2,8,215), S(3,8,215)]),
                LoggedExercise.Create(legCurl, 2, [S(1,8,52), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(legExtension, 3, [S(1,8,93), S(2,8,100), S(3,8,107)]),
                LoggedExercise.Create(calfRaise, 4, [S(1,8,90), S(2,8,70)]),
            ], loggedAt: new DateTime(2026, 1, 21, 12, 0, 0, DateTimeKind.Utc)),

            // 22/01/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,6,75), S(2,6,75), S(3,6,75)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,8,26), S(2,8,26), S(3,8,26)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,8,20), S(2,8,20)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,10,4.2m), S(2,10,4.2m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,20), S(2,8,22), S(3,8,22)]),
                LoggedExercise.Create(chestFly, 6, [S(1,8,86), S(2,8,93), S(3,8,93)]),
            ], loggedAt: new DateTime(2026, 1, 22, 12, 0, 0, DateTimeKind.Utc)),

            // 23/01/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(pullUp, 1, [S(1,6,0,0)]),
                LoggedExercise.Create(machineRow, 2, [S(1,8,73), S(2,8,73), S(3,8,73)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,6,12), S(2,6,14), S(3,6,14)]),
                LoggedExercise.Create(cableHammerCurl, 4, [S(1,8,18), S(2,8,22), S(3,8,22)]),
            ], loggedAt: new DateTime(2026, 1, 23, 12, 0, 0, DateTimeKind.Utc)),

            // 24/01/2026 - Bouldering
            LoggedWorkout.Create(SeededUserId, "Bouldering", [
                LoggedExercise.Create(bouldering, 1, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromHours(2), 0.5m, DistanceUnit.Km),
                ]),
            ], loggedAt: new DateTime(2026, 1, 24, 12, 0, 0, DateTimeKind.Utc)),

            // 25/01/2026 - Indoor Snowboarding
            LoggedWorkout.Create(SeededUserId, "Indoor Snowboarding", [
                LoggedExercise.Create(indoorSnowboarding, 1, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromHours(3), 0.5m, DistanceUnit.Km),
                ]),
            ], loggedAt: new DateTime(2026, 1, 25, 12, 0, 0, DateTimeKind.Utc)),

            // 29/01/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(inclineDumbbellPress, 1, [S(1,8,24), S(2,8,28), S(3,8,28)]),
                LoggedExercise.Create(chestPress, 2, [S(1,8,52), S(2,8,52), S(3,8,45)]),
                LoggedExercise.Create(dip, 3, [S(1,10,0,0), S(2,10,0,0)]),
                LoggedExercise.Create(machineShoulderPress, 4, [S(1,6,28), S(2,6,23)]),
                LoggedExercise.Create(lateralRaise, 5, [S(1,12,4.2m), S(2,12,4.2m)]),
                LoggedExercise.Create(tricepPushdown, 6, [S(1,8,20), S(2,8,20), S(3,8,22)]),
                LoggedExercise.Create(chestFly, 7, [S(1,8,86), S(2,8,86), S(3,8,100)]),
            ], loggedAt: new DateTime(2026, 1, 29, 12, 0, 0, DateTimeKind.Utc)),

            // 30/01/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(pullUp, 1, [S(1,7,0,0)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,8,12), S(2,8,14), S(3,8,14)]),
                LoggedExercise.Create(latPulldown, 4, [S(1,5,59), S(2,5,45)]),
                LoggedExercise.Create(wristCurl, 5, [S(1,8,18), S(2,8,24)]),
                LoggedExercise.Create(reverseFlye, 6, [S(1,8,45), S(2,8,52)]),
            ], loggedAt: new DateTime(2026, 1, 30, 12, 0, 0, DateTimeKind.Utc)),

            // 02/02/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legCurl, 1, [S(1,8,52), S(2,8,52), S(3,8,52)]),
                LoggedExercise.Create(legPress, 2, [S(1,10,175), S(2,10,215), S(3,10,215)]),
                LoggedExercise.Create(legExtension, 3, [S(1,8,73), S(2,8,93), S(3,8,93)]),
                LoggedExercise.Create(hipAdductor, 4, [S(1,12,45), S(2,12,52)]),
                LoggedExercise.Create(hipAbductor, 5, [S(1,12,45), S(2,12,52)]),
                LoggedExercise.Create(calfRaise, 6, [S(1,8,90), S(2,8,90)]),
                LoggedExercise.Create(sitUp, 7, [S(1,10,5), S(2,10,10)]),
            ], loggedAt: new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc)),

            // 03/02/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,5,77.5m), S(2,5,77.5m), S(3,5,70)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,8,26), S(2,8,26), S(3,8,24)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,6,20), S(2,6,20)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,10,4.2m), S(2,10,6.8m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,20), S(2,8,22), S(3,8,24)]),
                LoggedExercise.Create(overheadTricep, 6, [S(1,8,13), S(2,8,15)]),
                LoggedExercise.Create(chestFly, 7, [S(1,8,86), S(2,8,100), S(3,8,79)]),
            ], loggedAt: new DateTime(2026, 2, 3, 12, 0, 0, DateTimeKind.Utc)),

            // 04/02/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(latPulldown, 1, [S(1,8,66), S(2,8,66), S(3,8,73)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(preacherCurl, 3, [S(1,8,30), S(2,8,30), S(3,8,27.5m)]),
                LoggedExercise.Create(cableHammerCurl, 4, [S(1,10,18), S(2,10,22), S(3,10,20)]),
                LoggedExercise.Create(forearmCurl, 5, [S(1,12,24), S(2,12,27), S(3,12,24)]),
            ], loggedAt: new DateTime(2026, 2, 4, 12, 0, 0, DateTimeKind.Utc)),

            // 06/02/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(hackSquat, 1, [S(1,8,70), S(2,8,80), S(3,8,80)]),
                LoggedExercise.Create(legPress, 2, [S(1,8,195), S(2,8,195), S(3,8,235)]),
                LoggedExercise.Create(legCurl, 3, [S(1,10,52), S(2,10,59), S(3,10,59)]),
                LoggedExercise.Create(legExtension, 4, [S(1,8,79), S(2,8,79), S(3,8,86)]),
                LoggedExercise.Create(hipAdductor, 5, [S(1,10,45), S(2,10,52)]),
                LoggedExercise.Create(hipAbductor, 6, [S(1,10,52), S(2,10,66)]),
                LoggedExercise.Create(calfRaise, 7, [S(1,10,70), S(2,10,90)]),
            ], loggedAt: new DateTime(2026, 2, 6, 12, 0, 0, DateTimeKind.Utc)),

            // 08/02/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,6,75), S(2,6,75), S(3,6,70)]),
                LoggedExercise.Create(dip, 2, [S(1,10,0,0), S(2,10,0,0), S(3,10,0,0)]),
                LoggedExercise.Create(inclineDumbbellPress, 3, [S(1,6,26), S(2,6,24), S(3,6,24)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,8,4.8m), S(2,8,6.8m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,20), S(2,8,22), S(3,8,22)]),
                LoggedExercise.Create(chestFly, 6, [S(1,10,86), S(2,10,86), S(3,10,100)]),
            ], loggedAt: new DateTime(2026, 2, 8, 12, 0, 0, DateTimeKind.Utc)),

            // 10/02/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,52), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,8,59), S(2,8,66), S(3,8,59)]),
                LoggedExercise.Create(cableCurl, 3, [S(1,8,18), S(2,8,24), S(3,8,22)]),
                LoggedExercise.Create(cableHammerCurl, 4, [S(1,8,18), S(2,8,15)]),
                LoggedExercise.Create(reverseFlye, 5, [S(1,10,4.2m), S(2,10,4.2m)]),
                LoggedExercise.Create(forearmCurl, 6, [S(1,10,22), S(2,10,24), S(3,10,36)]),
            ], loggedAt: new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc)),

            // 12/02/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legCurl, 1, [S(1,8,52), S(2,8,59), S(3,8,55)]),
                LoggedExercise.Create(legPress, 2, [S(1,8,215), S(2,8,235), S(3,8,235)]),
                LoggedExercise.Create(legExtension, 3, [S(1,8,79), S(2,8,86), S(3,8,86)]),
            ], loggedAt: new DateTime(2026, 2, 12, 12, 0, 0, DateTimeKind.Utc)),

            // 14/02/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,6,60), S(2,6,70), S(3,6,80)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,10,26), S(2,10,26), S(3,10,26)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,8,24), S(2,8,32)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,8,6.8m), S(2,8,6.8m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,20), S(2,8,22), S(3,8,22)]),
                LoggedExercise.Create(chestFly, 6, [S(1,8,86), S(2,8,86), S(3,8,86)]),
            ], loggedAt: new DateTime(2026, 2, 14, 12, 0, 0, DateTimeKind.Utc)),

            // 16/02/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(pullUp, 1, [S(1,4,0,0), S(2,4,0,0)]),
                LoggedExercise.Create(tbarRow, 2, [S(1,8,40), S(2,8,60), S(3,8,60)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,6,14), S(2,6,14), S(3,6,14)]),
                LoggedExercise.Create(latPulldown, 4, [S(1,6,59), S(2,6,52)]),
                LoggedExercise.Create(seatedCableRow, 5, [S(1,8,59), S(2,8,59)]),
                LoggedExercise.Create(hammerCurl, 6, [S(1,8,18), S(2,8,22)]),
                LoggedExercise.Create(forearmCurl, 7, [S(1,10,24), S(2,10,27), S(3,10,29)]),
            ], loggedAt: new DateTime(2026, 2, 16, 12, 0, 0, DateTimeKind.Utc)),

            // 18/02/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,8,75), S(2,8,75), S(3,8,70)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,8,26), S(2,8,28), S(3,8,26)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,8,22), S(2,8,20)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,12,6.8m), S(2,12,5.2m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,25), S(2,8,27.5m)]),
                LoggedExercise.Create(chestFly, 6, [S(1,10,0,0), S(2,10,0,0), S(3,10,0,0)]),
                LoggedExercise.Create(overheadTricep, 7, [S(1,8,15), S(2,8,18), S(3,8,18)]),
            ], loggedAt: new DateTime(2026, 2, 18, 12, 0, 0, DateTimeKind.Utc)),

            // 21/02/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(latPulldown, 1, [S(1,8,52), S(2,8,59), S(3,8,66), S(4,8,66)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,8,60.5m), S(2,8,60.5m), S(3,8,59)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,6,14), S(2,6,14)]),
                LoggedExercise.Create(cableHammerCurl, 4, [S(1,8,18), S(2,8,22), S(3,8,20)]),
                LoggedExercise.Create(forearmCurl, 5, [S(1,10,24), S(2,10,27), S(3,10,33)]),
                LoggedExercise.Create(reverseFlye, 6, [S(1,12,4.2m), S(2,12,6.8m)]),
            ], loggedAt: new DateTime(2026, 2, 21, 12, 0, 0, DateTimeKind.Utc)),

            // 22/02/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legExtension, 1, [S(1,8,79), S(2,8,79), S(3,8,93)]),
                LoggedExercise.Create(legCurl, 2, [S(1,8,52), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(calfRaise, 3, [S(1,8,80), S(2,8,80), S(3,8,80)]),
            ], loggedAt: new DateTime(2026, 2, 22, 12, 0, 0, DateTimeKind.Utc)),

            // 23/02/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,6,70), S(2,6,80), S(3,6,77.5m), S(4,6,70)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,8,26), S(2,8,26), S(3,8,26)]),
                LoggedExercise.Create(machineShoulderPress, 3, [S(1,8,32), S(2,8,27), S(3,8,27)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,8,6.8m), S(2,8,6.8m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,10,24), S(2,10,29), S(3,10,29)]),
                LoggedExercise.Create(chestFly, 6, [S(1,8,86), S(2,8,93)]),
            ], loggedAt: new DateTime(2026, 2, 23, 12, 0, 0, DateTimeKind.Utc)),

            // 25/02/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(latPulldown, 1, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,6,59), S(2,6,59), S(3,6,66)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,6,14), S(2,6,14), S(3,6,14)]),
                LoggedExercise.Create(cableCurl, 4, [S(1,10,20), S(2,10,18)]),
                LoggedExercise.Create(reverseFlye, 5, [S(1,12,4.2m), S(2,12,5.8m)]),
            ], loggedAt: new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc)),

            // 26/02/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(hackSquat, 1, [S(1,8,65), S(2,8,75)]),
                LoggedExercise.Create(legPress, 2, [S(1,8,195), S(2,8,215), S(3,8,235)]),
                LoggedExercise.Create(legCurl, 3, [S(1,8,52), S(2,8,52), S(3,8,59)]),
                LoggedExercise.Create(legExtension, 4, [S(1,10,86), S(2,10,86), S(3,10,93)]),
                LoggedExercise.Create(hipAdductor, 5, [S(1,12,45), S(2,12,52)]),
                LoggedExercise.Create(hipAbductor, 6, [S(1,12,52), S(2,12,59)]),
            ], loggedAt: new DateTime(2026, 2, 26, 12, 0, 0, DateTimeKind.Utc)),

            // 28/02/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,8,80), S(2,8,75), S(3,8,60), S(4,8,60)]),
                LoggedExercise.Create(dip, 2, [S(1,10,0,0), S(2,10,0,0), S(3,10,0,0)]),
                LoggedExercise.Create(inclineDumbbellPress, 3, [S(1,6,26), S(2,6,24)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,10,6.8m), S(2,10,4.2m), S(3,10,4.2m)]),
                LoggedExercise.Create(chestFly, 5, [S(1,8,93), S(2,8,79)]),
                LoggedExercise.Create(tricepPushdown, 6, [S(1,10,22), S(2,10,18)]),
            ], loggedAt: new DateTime(2026, 2, 28, 12, 0, 0, DateTimeKind.Utc)),

            // 01/03/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(latPulldown, 1, [S(1,8,59), S(2,8,66), S(3,8,73)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,8,66), S(2,8,66), S(3,8,66)]),
                LoggedExercise.Create(preacherCurl, 3, [S(1,8,30), S(2,8,30)]),
                LoggedExercise.Create(cableCurl, 4, [S(1,10,20), S(2,10,18)]),
                LoggedExercise.Create(forearmCurl, 5, [S(1,8,24), S(2,8,36)]),
                LoggedExercise.Create(reverseFlye, 6, [S(1,12,4.2m), S(2,12,5.7m)]),
            ], loggedAt: new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc)),

            // 02/03/2026 - Cardio (Run: 4.1km, avg 5:22/km ≈ 22min)
            LoggedWorkout.Create(SeededUserId, "Cardio", [
                LoggedExercise.Create(treadmill, 1, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromMinutes(22), 4.1m, DistanceUnit.Km),
                ]),
            ], loggedAt: new DateTime(2026, 3, 2, 12, 0, 0, DateTimeKind.Utc)),

            // 04/03/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(inclineDumbbellPress, 1, [S(1,8,22), S(2,8,28), S(3,8,28)]),
                LoggedExercise.Create(chestFly, 2, [S(1,8,93), S(2,8,86), S(3,8,100)]),
                LoggedExercise.Create(machineShoulderPress, 3, [S(1,6,32), S(2,6,32), S(3,6,27)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,12,4.2m), S(2,12,5.7m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,18), S(2,8,20), S(3,8,20)]),
                LoggedExercise.Create(overheadTricep, 6, [S(1,6,15)]),
                LoggedExercise.Create(benchPress, 7, [S(1,8,45), S(2,8,52)]),
            ], loggedAt: new DateTime(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc)),

            // 05/03/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,59), S(2,8,66), S(3,8,73)]),
                LoggedExercise.Create(bayesianCurl, 2, [S(1,8,9), S(2,8,13)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,6,14), S(2,6,14), S(3,6,12)]),
                LoggedExercise.Create(latPulldown, 4, [S(1,8,59), S(2,8,52)]),
                LoggedExercise.Create(forearmCurl, 5, [S(1,8,27), S(2,8,29), S(3,8,31)]),
                LoggedExercise.Create(reverseFlye, 6, [S(1,10,4.8m), S(2,10,4.8m)]),
                LoggedExercise.Create(machineRow, 7, [S(1,8,52), S(2,8,66)]),
                LoggedExercise.Create(treadmill, 8, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromMinutes(13), 1.2m, DistanceUnit.Km),
                ]),
            ], loggedAt: new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc)),

            // 09/03/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,5,75), S(2,5,75), S(3,5,77.5m)]),
                LoggedExercise.Create(overheadPress, 2, [S(1,8,22), S(2,8,22), S(3,8,22)]),
                LoggedExercise.Create(dip, 3, [S(1,8,0,0), S(2,8,0,0)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,10,4.2m), S(2,10,6.8m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,18), S(2,8,20), S(3,8,22)]),
                LoggedExercise.Create(chestFly, 6, [S(1,8,86), S(2,8,86), S(3,8,83)]),
            ], loggedAt: new DateTime(2026, 3, 9, 12, 0, 0, DateTimeKind.Utc)),

            // 11/03/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,59), S(2,8,66), S(3,8,66)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,6,59), S(2,6,59), S(3,6,66)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,6,14), S(2,6,14), S(3,6,12)]),
                LoggedExercise.Create(facePull, 4, [S(1,8,15), S(2,8,15), S(3,8,18), S(4,8,13)]),
                LoggedExercise.Create(hammerCurl, 5, [S(1,8,18), S(2,8,20), S(3,8,22)]),
                LoggedExercise.Create(forearmCurl, 6, [S(1,8,27), S(2,8,27), S(3,8,27)]),
            ], loggedAt: new DateTime(2026, 3, 11, 12, 0, 0, DateTimeKind.Utc)),

            // 13/03/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(benchPress, 1, [S(1,4,60), S(2,4,80), S(3,4,90)]),
                LoggedExercise.Create(squat, 2, [S(1,6,60), S(2,6,80)]),
                LoggedExercise.Create(legExtension, 3, [S(1,8,79), S(2,8,93), S(3,8,93)]),
                LoggedExercise.Create(legCurl, 4, [S(1,8,59), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(calfRaise, 5, [S(1,6,80)]),
            ], loggedAt: new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc)),

            // 16/03/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,5,75), S(2,5,77.5m), S(3,5,77.5m)]),
                LoggedExercise.Create(overheadPress, 2, [S(1,8,24), S(2,8,24), S(3,8,22)]),
                LoggedExercise.Create(dip, 3, [S(1,8,0,0), S(2,8,0,0)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,10,4.2m), S(2,10,4.2m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,7,20), S(2,7,18), S(3,7,18)]),
                LoggedExercise.Create(chestFly, 6, [S(1,7,86), S(2,7,86), S(3,7,79)]),
            ], loggedAt: new DateTime(2026, 3, 16, 12, 0, 0, DateTimeKind.Utc)),

            // 17/03/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,59), S(2,8,66), S(3,8,66)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,6,59), S(2,6,59), S(3,6,59)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,5,14), S(2,5,14), S(3,5,12)]),
                LoggedExercise.Create(facePull, 4, [S(1,8,15), S(2,8,13), S(3,8,15)]),
                LoggedExercise.Create(hammerCurl, 5, [S(1,8,20), S(2,8,22), S(3,8,20)]),
            ], loggedAt: new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc)),

            // 18/03/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(squat, 1, [S(1,6,60), S(2,6,70), S(3,6,70)]),
                LoggedExercise.Create(hipAdductor, 2, [S(1,12,59), S(2,12,66)]),
                LoggedExercise.Create(hipAbductor, 3, [S(1,8,52), S(2,8,52)]),
                LoggedExercise.Create(legCurl, 4, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(legExtension, 5, [S(1,6,86), S(2,6,86), S(3,6,86)]),
                LoggedExercise.Create(calfRaise, 6, [S(1,8,80), S(2,8,80), S(3,8,90)]),
                LoggedExercise.Create(machineCrunch, 7, [S(1,8,66), S(2,8,59)]),
            ], loggedAt: new DateTime(2026, 3, 18, 12, 0, 0, DateTimeKind.Utc)),

            // 20/03/2026 - Upper
            LoggedWorkout.Create(SeededUserId, "Upper", [
                LoggedExercise.Create(inclineDumbbellPress, 1, [S(1,8,26), S(2,8,30), S(3,8,28)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,8,66), S(2,8,73), S(3,8,73)]),
                LoggedExercise.Create(dip, 3, [S(1,8,5), S(2,8,20)]),
                LoggedExercise.Create(inclineDumbbellCurl, 4, [S(1,6,14), S(2,6,14), S(3,6,16)]),
                LoggedExercise.Create(seatedCableRow, 5, [S(1,6,66), S(2,6,66), S(3,6,66)]),
                LoggedExercise.Create(tricepPushdown, 6, [S(1,8,20), S(2,8,22), S(3,8,22)]),
            ], loggedAt: new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc)),

            // 21/03/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legPress, 1, [S(1,8,120), S(2,8,160), S(3,8,140)]),
                LoggedExercise.Create(legExtension, 2, [S(1,8,73), S(2,8,86), S(3,8,93)]),
                LoggedExercise.Create(hipAdductor, 3, [S(1,10,52), S(2,10,59)]),
                LoggedExercise.Create(hipAbductor, 4, [S(1,10,59), S(2,10,59)]),
                LoggedExercise.Create(calfRaise, 5, [S(1,8,70), S(2,8,70), S(3,8,70)]),
                LoggedExercise.Create(legCurl, 6, [S(1,8,52), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(crunch, 7, [S(1,8,0,0), S(2,8,0,0), S(3,8,0,0)]),
            ], loggedAt: new DateTime(2026, 3, 21, 12, 0, 0, DateTimeKind.Utc)),

            // 23/03/2026 - Push (+ Run)
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(treadmill, 1, [
                    LoggedSet.CreateCardio(1, TimeSpan.FromMinutes(36), 6.0m, DistanceUnit.Km),
                ]),
                LoggedExercise.Create(benchPress, 2, [S(1,5,77.5m), S(2,5,77.5m), S(3,5,77.5m)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,8,24), S(2,8,24), S(3,8,20)]),
                LoggedExercise.Create(dip, 4, [S(1,8,0,0), S(2,8,0,0), S(3,8,0,0)]),
                LoggedExercise.Create(lateralRaise, 5, [S(1,10,4.8m), S(2,10,6.2m), S(3,10,4.8m)]),
                LoggedExercise.Create(tricepPushdown, 6, [S(1,8,20), S(2,8,22), S(3,8,22)]),
                LoggedExercise.Create(chestFly, 7, [S(1,8,86), S(2,8,93), S(3,8,86)]),
            ], loggedAt: new DateTime(2026, 3, 23, 12, 0, 0, DateTimeKind.Utc)),

            // 25/03/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(latPulldown, 1, [S(1,8,59), S(2,8,66), S(3,8,66), S(4,8,59)]),
                LoggedExercise.Create(seatedCableRow, 2, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,8,14), S(2,8,14), S(3,8,14)]),
                LoggedExercise.Create(facePull, 4, [S(1,6,15), S(2,6,15), S(3,6,15)]),
                LoggedExercise.Create(cableHammerCurl, 5, [S(1,8,20), S(2,8,20), S(3,8,22)]),
                LoggedExercise.Create(forearmCurl, 6, [S(1,8,24), S(2,8,27), S(3,8,33)]),
                LoggedExercise.Create(reverseForearmCurl, 7, [S(1,10,6.8m), S(2,10,6.8m)]),
            ], loggedAt: new DateTime(2026, 3, 25, 12, 0, 0, DateTimeKind.Utc)),

            // 26/03/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legCurl, 1, [S(1,8,52), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(squat, 2, [S(1,8,60), S(2,8,70), S(3,8,80)]),
                LoggedExercise.Create(calfPress, 3, [S(1,8,70), S(2,8,70), S(3,8,70)]),
                LoggedExercise.Create(machineCrunch, 4, [S(1,10,52), S(2,10,52), S(3,10,48)]),
                LoggedExercise.Create(legPress, 5, [S(1,8,120), S(2,8,140)]),
            ], loggedAt: new DateTime(2026, 3, 26, 12, 0, 0, DateTimeKind.Utc)),

            // 27/03/2026 - Upper
            LoggedWorkout.Create(SeededUserId, "Upper", [
                LoggedExercise.Create(latPulldown, 1, [S(1,8,59), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,8,26), S(2,8,28), S(3,8,28)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,6,22), S(2,6,22)]),
                LoggedExercise.Create(inclineDumbbellCurl, 4, [S(1,8,14), S(2,8,14), S(3,8,14)]),
                LoggedExercise.Create(seatedCableRow, 5, [S(1,8,59), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(overheadTricep, 6, [S(1,8,15), S(2,8,15), S(3,8,15)]),
                LoggedExercise.Create(chestFly, 7, [S(1,8,86), S(2,8,100), S(3,8,100)]),
            ], loggedAt: new DateTime(2026, 3, 27, 12, 0, 0, DateTimeKind.Utc)),

            // 28/03/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(benchPress, 1, [S(1,8,60), S(2,8,70)]),
                LoggedExercise.Create(squat, 2, [S(1,5,60), S(2,5,80), S(3,5,80)]),
                LoggedExercise.Create(legExtension, 3, [S(1,8,86), S(2,8,86), S(3,8,93)]),
                LoggedExercise.Create(legCurl, 4, [S(1,8,59), S(2,8,66), S(3,8,59)]),
                LoggedExercise.Create(calfRaise, 5, [S(1,10,55), S(2,10,80)]),
                LoggedExercise.Create(crunch, 6, [S(1,10,0,0), S(2,10,0,0), S(3,10,0,0)]),
            ], loggedAt: new DateTime(2026, 3, 28, 12, 0, 0, DateTimeKind.Utc)),

            // 30/03/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,5,80), S(2,5,80), S(3,5,77.5m)]),
                LoggedExercise.Create(overheadPress, 2, [S(1,8,24), S(2,8,24), S(3,8,24)]),
                LoggedExercise.Create(lateralRaise, 3, [S(1,8,6.8m), S(2,8,5.2m)]),
                LoggedExercise.Create(tricepPushdown, 4, [S(1,8,22), S(2,8,22), S(3,8,20)]),
                LoggedExercise.Create(dip, 5, [S(1,8,0,0), S(2,8,0,0)]),
                LoggedExercise.Create(chestFly, 6, [S(1,8,93), S(2,8,93), S(3,8,79)]),
            ], loggedAt: new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc)),

            // 31/03/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,73), S(2,8,73), S(3,8,66)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,8,66), S(2,8,59), S(3,8,59)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,8,14), S(2,8,14), S(3,8,14)]),
                LoggedExercise.Create(facePull, 4, [S(1,8,15), S(2,8,15), S(3,8,18), S(4,8,18)]),
                LoggedExercise.Create(hammerCurl, 5, [S(1,8,18), S(2,8,22), S(3,8,22)]),
                LoggedExercise.Create(wristCurl, 6, [S(1,10,24), S(2,10,27), S(3,10,29)]),
                LoggedExercise.Create(wristCurl, 7, [S(1,8,11), S(2,8,9), S(3,8,9)]),
            ], loggedAt: new DateTime(2026, 3, 31, 12, 0, 0, DateTimeKind.Utc)),

            // 03/04/2026 - Upper
            LoggedWorkout.Create(SeededUserId, "Upper", [
                LoggedExercise.Create(inclineDumbbellPress, 1, [S(1,8,24), S(2,8,30), S(3,8,30), S(4,8,26)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,8,66), S(2,8,66), S(3,8,73)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,8,39), S(2,8,41), S(3,8,39)]),
                LoggedExercise.Create(preacherCurl, 4, [S(1,8,32), S(2,8,30), S(3,8,25)]),
                LoggedExercise.Create(chestSupportedRow, 5, [S(1,10,30), S(2,10,30), S(3,10,25), S(4,10,25)]),
            ], loggedAt: new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc)),

            // 04/04/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legPress, 1, [S(1,8,120), S(2,8,120), S(3,8,160)]),
                LoggedExercise.Create(legCurl, 2, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(legExtension, 3, [S(1,8,86), S(2,8,100), S(3,8,100)]),
                LoggedExercise.Create(calfRaise, 4, [S(1,8,70), S(2,8,70), S(3,8,70)]),
            ], loggedAt: new DateTime(2026, 4, 4, 12, 0, 0, DateTimeKind.Utc)),

            // 06/04/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,5,80), S(2,5,80), S(3,5,82.5m)]),
                LoggedExercise.Create(overheadPress, 2, [S(1,6,22), S(2,6,22), S(3,6,22)]),
                LoggedExercise.Create(dip, 3, [S(1,10,0,0), S(2,10,0,0)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,8,4.8m), S(2,8,4.8m), S(3,8,4.8m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,6,22), S(2,6,20), S(3,6,20)]),
                LoggedExercise.Create(chestFly, 6, [S(1,6,93), S(2,6,93), S(3,6,93)]),
            ], loggedAt: new DateTime(2026, 4, 6, 12, 0, 0, DateTimeKind.Utc)),

            // 07/04/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,66), S(2,8,66), S(3,8,73)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,6,59), S(2,6,52), S(3,6,59)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,6,14), S(2,6,14), S(3,6,14)]),
                LoggedExercise.Create(facePull, 4, [S(1,8,15), S(2,8,18), S(3,8,18)]),
                LoggedExercise.Create(cableHammerCurl, 5, [S(1,8,20), S(2,8,20), S(3,8,20)]),
                LoggedExercise.Create(forearmCurl, 6, [S(1,8,28), S(2,8,24), S(3,8,22)]),
            ], loggedAt: new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc)),

            // 08/04/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(squat, 1, [S(1,5,70), S(2,5,70), S(3,5,60)]),
                LoggedExercise.Create(legCurl, 2, [S(1,8,59), S(2,8,59), S(3,8,66)]),
                LoggedExercise.Create(calfRaise, 3, [S(1,8,70), S(2,8,70), S(3,8,70)]),
                LoggedExercise.Create(legPress, 4, [S(1,8,120), S(2,8,160), S(3,8,180)]),
            ], loggedAt: new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc)),

            // 13/04/2026 - Push
            LoggedWorkout.Create(SeededUserId, "Push", [
                LoggedExercise.Create(benchPress, 1, [S(1,5,60), S(2,5,95), S(3,5,80)]),
                LoggedExercise.Create(inclineDumbbellPress, 2, [S(1,8,28), S(2,8,28), S(3,8,26)]),
                LoggedExercise.Create(overheadPress, 3, [S(1,8,22), S(2,8,24)]),
                LoggedExercise.Create(lateralRaise, 4, [S(1,8,4.8m), S(2,8,6.2m)]),
                LoggedExercise.Create(tricepPushdown, 5, [S(1,8,22), S(2,8,24), S(3,8,20)]),
            ], loggedAt: new DateTime(2026, 4, 13, 12, 0, 0, DateTimeKind.Utc)),

            // 14/04/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,68), S(2,8,75), S(3,8,68)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,8,54), S(2,8,65), S(3,8,58)]),
                LoggedExercise.Create(inclineDumbbellCurl, 3, [S(1,8,14), S(2,8,14), S(3,8,14)]),
                LoggedExercise.Create(facePull, 4, [S(1,8,16), S(2,8,20), S(3,8,16)]),
                LoggedExercise.Create(cableHammerCurl, 5, [S(1,8,20), S(2,8,20)]),
                LoggedExercise.Create(forearmCurl, 6, [S(1,8,24), S(2,8,27)]),
            ], loggedAt: new DateTime(2026, 4, 14, 12, 0, 0, DateTimeKind.Utc)),

            // 16/04/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(legCurl, 1, [S(1,6,52), S(2,6,59), S(3,6,66), S(4,6,52)]),
                LoggedExercise.Create(legExtension, 2, [S(1,8,79), S(2,8,93), S(3,8,93)]),
                LoggedExercise.Create(legPress, 3, [S(1,8,120), S(2,8,160), S(3,8,150)]),
                LoggedExercise.Create(calfRaise, 4, [S(1,8,70), S(2,8,70), S(3,8,70)]),
            ], loggedAt: new DateTime(2026, 4, 16, 12, 0, 0, DateTimeKind.Utc)),

            // 02/05/2026 - Legs
            LoggedWorkout.Create(SeededUserId, "Legs", [
                LoggedExercise.Create(benchPress, 1, [S(1,6,95), S(2,6,75), S(3,6,75)]),
                LoggedExercise.Create(legExtension, 2, [S(1,8,79), S(2,8,100), S(3,8,83)]),
                LoggedExercise.Create(legCurl, 3, [S(1,8,59), S(2,8,59), S(3,8,52)]),
            ], loggedAt: new DateTime(2026, 5, 2, 12, 0, 0, DateTimeKind.Utc)),

            // 10/05/2026 - Pull
            LoggedWorkout.Create(SeededUserId, "Pull", [
                LoggedExercise.Create(seatedCableRow, 1, [S(1,8,55), S(2,8,70), S(3,8,65)]),
                LoggedExercise.Create(latPulldown, 2, [S(1,8,73), S(2,8,66), S(3,8,59), S(4,8,59)]),
                LoggedExercise.Create(preacherCurl, 3, [S(1,6,30), S(2,6,30)]),
                LoggedExercise.Create(inclineDumbbellCurl, 4, [S(1,6,14), S(2,6,14)]),
                LoggedExercise.Create(hammerCurl, 5, [S(1,8,24), S(2,8,22), S(3,8,22)]),
                LoggedExercise.Create(reverseFlye, 6, [S(1,10,4.2m), S(2,10,6.8m)]),
            ], loggedAt: new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc)),
        };

        context.LoggedWorkouts.AddRange(logs);
        await context.SaveChangesAsync();
    }
}
