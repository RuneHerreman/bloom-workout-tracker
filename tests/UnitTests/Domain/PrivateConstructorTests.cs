using System.Reflection;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace UnitTests.Domain;

// EF Core requires parameterless constructors on entities and value objects.
// These tests instantiate them via reflection so coverage tracks them.
public sealed class PrivateConstructorTests
{
    private static T InvokeParameterless<T>()
    {
        var ctor = typeof(T).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        Assert.NotNull(ctor);
        return (T)ctor!.Invoke(null);
    }

    [Fact]
    public void Exercise_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<Exercise>());

    [Fact]
    public void ExerciseName_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<ExerciseName>());

    [Fact]
    public void ExerciseDescription_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<ExerciseDescription>());

    [Fact]
    public void TargetMuscle_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<TargetMuscle>());

    [Fact]
    public void LoggedWorkout_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<LoggedWorkout>());

    [Fact]
    public void LoggedExercise_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<LoggedExercise>());

    [Fact]
    public void LoggedSet_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<LoggedSet>());

    [Fact]
    public void LoggedStrengthSet_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<LoggedStrengthSet>());

    [Fact]
    public void Distance_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<Distance>());

    [Fact]
    public void Duration_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<Duration>());

    [Fact]
    public void Reps_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<Reps>());

    [Fact]
    public void RIR_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<RIR>());

    [Fact]
    public void Weight_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<Weight>());

    [Fact]
    public void User_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<User>());

    [Fact]
    public void Email_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<Email>());

    [Fact]
    public void HashedPassword_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<HashedPassword>());

    [Fact]
    public void Username_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<Username>());

    [Fact]
    public void WorkoutTemplate_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<WorkoutTemplate>());

    [Fact]
    public void TemplateExercise_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<TemplateExercise>());

    [Fact]
    public void PlannedSet_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<PlannedSet>());

    [Fact]
    public void PlannedDistance_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<PlannedDistance>());

    [Fact]
    public void PlannedDuration_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<PlannedDuration>());

    [Fact]
    public void PlannedReps_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<PlannedReps>());

    [Fact]
    public void WorkoutTemplateName_PrivateCtor_ShouldExist() => Assert.NotNull(InvokeParameterless<WorkoutTemplateName>());
}
