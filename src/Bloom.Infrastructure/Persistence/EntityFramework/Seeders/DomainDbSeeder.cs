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
            User.Create("frans.appelmans@gmail.com", "FransAppelmans", passwordHasher.HashPassword("test"), "Frans", "Appelmans", 80m, 180, 3, EntityId.New<UserId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490000"))),
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
        var allLogs = await context.LoggedWorkouts.ToListAsync();
        var existingLogs = allLogs.Where(w => w.UserId.Value == SeededUserId.Value).ToList();
        context.LoggedWorkouts.RemoveRange(existingLogs);
        await context.SaveChangesAsync();

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