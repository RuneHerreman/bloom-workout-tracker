using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Seeders;

public class DomainDbSeeder(DomainDbContext context, ILogger<DomainDbSeeder> logger)
{
    public async Task Seed()
    {
        await SeedExercises();
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
            Exercise.Create("Squat", "A compound lower body exercise targeting the quads, glutes and hamstrings.", ExerciseType.Strength, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490026"))),
            Exercise.Create("Leg Press", "A compound lower body exercise targeting the quads and glutes.", ExerciseType.Strength, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490027"))),
            Exercise.Create("Leg Extension", "An isolation exercise targeting the quadriceps.", ExerciseType.Strength, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490028"))),
            Exercise.Create("Bulgarian Split Squat", "A unilateral compound exercise targeting the quads and glutes.", ExerciseType.Strength, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490029"))),
            Exercise.Create("Hack Squat", "A compound lower body exercise targeting the quads.", ExerciseType.Strength, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490030"))),
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
            Exercise.Create("Lunge", "A unilateral compound exercise targeting the quads and glutes.", ExerciseType.Strength, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490048"))),
            Exercise.Create("Sumo Deadlift", "A wide stance deadlift variation targeting the inner thighs and glutes.", ExerciseType.Strength, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490049"))),
            Exercise.Create("Incline Dumbbell Curl", "An isolation exercise targeting the biceps with a stretch at the bottom.", ExerciseType.Strength, ["Biceps"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490050"))),

            // Cardio
            Exercise.Create("Treadmill Run", "A sustained cardiovascular exercise performed on a treadmill.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490051"))),
            Exercise.Create("Cycling", "A low impact cardiovascular exercise performed on a bike.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490052"))),
            Exercise.Create("Rowing Machine", "A full body cardiovascular exercise performed on a rowing ergometer.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490053"))),
            Exercise.Create("Stair Climber", "A cardiovascular exercise targeting the lower body using a stair machine.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490054"))),
            Exercise.Create("Elliptical", "A low impact full body cardiovascular exercise.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490055"))),
            Exercise.Create("Jump Rope", "A high intensity cardiovascular exercise using a skipping rope.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490056"))),
            Exercise.Create("Swimming", "A full body low impact cardiovascular exercise performed in water.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490057"))),
            Exercise.Create("Battle Ropes", "A high intensity cardiovascular exercise using heavy ropes.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490058"))),
            Exercise.Create("Assault Bike", "A full body high intensity cardiovascular exercise on an air bike.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490059"))),
            Exercise.Create("Sled Push", "A functional cardiovascular exercise pushing a weighted sled.", ExerciseType.Cardio, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490060"))),

            // Plyometric
            Exercise.Create("Box Jump", "An explosive lower body exercise jumping onto a raised platform.", ExerciseType.Plyometric, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490061"))),
            Exercise.Create("Depth Jump", "An advanced plyometric exercise stepping off a box and immediately jumping.", ExerciseType.Plyometric, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490062"))),
            Exercise.Create("Broad Jump", "An explosive horizontal jump targeting the lower body.", ExerciseType.Plyometric, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490063"))),
            Exercise.Create("Lateral Bound", "An explosive lateral jump targeting the glutes and abductors.", ExerciseType.Plyometric, ["Glutes"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490064"))),
            Exercise.Create("Clap Push Up", "An explosive upper body plyometric exercise.", ExerciseType.Plyometric, ["Chest"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490065"))),
            Exercise.Create("Tuck Jump", "An explosive jump bringing the knees to the chest at the top.", ExerciseType.Plyometric, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490066"))),
            Exercise.Create("Burpee", "A full body plyometric exercise combining a squat, push up and jump.", ExerciseType.Plyometric, ["Full Body"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490067"))),
            Exercise.Create("Medicine Ball Slam", "An explosive full body exercise slamming a medicine ball to the ground.", ExerciseType.Plyometric, ["Core"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490068"))),
            Exercise.Create("Single Leg Hop", "A unilateral plyometric exercise targeting the quads and glutes.", ExerciseType.Plyometric, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490069"))),
            Exercise.Create("Hurdle Jump", "An explosive jump over a hurdle targeting the lower body.", ExerciseType.Plyometric, ["Quads"], EntityId.New<ExerciseId>(Guid.Parse("019d059e-d220-71db-8a1a-ec7569490070"))),
    
        };

        context.Exercises.AddRange(exercises);
        await context.SaveChangesAsync();
    }
}
