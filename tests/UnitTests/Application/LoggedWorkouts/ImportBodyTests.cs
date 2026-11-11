using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Body;
using Bloom.Domain.Users;
using UnitTests.Application.Exercises;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

/// <summary>Body log days in an import: a Bloom backup's Body sheet or a scale app's file.</summary>
public sealed class ImportBodyTests : ApplicationTestBase
{
    private static readonly DateOnly Oct1 = new(2026, 10, 1);

    private async Task<(User User, ImportLoggedWorkouts UseCase)> Setup(decimal weight = 80m)
    {
        var user = User.Create("scale@example.com", "sam", "hash", "Sam", "Lee", weight, 175, 3, new DateOnly(1991, 4, 4));
        await UserRepository.Save(user);
        var useCase = new ImportLoggedWorkouts(UnitOfWork, StubCurrentUser.With(user.Id),
            new MockFindLoggedWorkoutsQuery([]), new MockSearchExerciseCatalogQuery([]), CreateLogger<ImportLoggedWorkouts>());
        return (user, useCase);
    }

    private static ImportLoggedWorkoutsInput BodyOnly(params ImportBodyInput[] days) => new([], "scale.csv", Body: days.ToList());

    [Fact]
    public async Task BodyOnlyImport_ShouldAddTheDays_RecordThemForUndo_AndUpdateCurrentWeight()
    {
        var (user, useCase) = await Setup();

        var output = await useCase.Execute(BodyOnly(
            new(Oct1, 81.2m, 19.5m),
            new(Oct1.AddDays(1), 81m),
            new(Oct1.AddDays(2), 80.6m, 19.2m)));

        Assert.Equal((3, 0, 0), (output.Body!.Imported, output.Body.Skipped, output.Body.DroppedBodyFat));
        Assert.Empty(output.Body.Failures);
        var entries = await BodyMeasurementRepository.ForUser(user.Id);
        Assert.Equal(3, entries.Count);
        Assert.All(entries, e => Assert.Equal(BodyMeasurementSource.Import, e.Source));
        Assert.Equal(80.6m, (await UserRepository.ById(user.Id)).Value.Weight);

        var batch = (await ImportBatchRepository.ForUser(user.Id)).Single();
        Assert.Equal(output.BatchId, batch.Id.Value);
        Assert.Equal(entries.Select(e => e.Id.Value).OrderBy(x => x), batch.BodyMeasurementIds.OrderBy(x => x));
        Assert.Empty(batch.WorkoutIds);
    }

    [Fact]
    public async Task DaysAlreadyInTheLog_OrRepeatedInTheFile_ShouldBeSkipped_NotOverwritten()
    {
        var (user, useCase) = await Setup();
        await BodyMeasurementRepository.Save(BodyMeasurement.Create(user.Id, Oct1, 79m, 18m, BodyMeasurementSource.Manual));

        var output = await useCase.Execute(BodyOnly(new(Oct1, 85m), new(Oct1.AddDays(1), 80m), new(Oct1.AddDays(1), 90m)));

        Assert.Equal(1, output.Body!.Imported);
        Assert.Equal(2, output.Body.Skipped);
        var entries = await BodyMeasurementRepository.ForUser(user.Id);
        Assert.Equal([79m, 80m], entries.Select(e => e.WeightKg));
        Assert.Equal(18m, entries[0].BodyFatPct);
    }

    [Fact]
    public async Task BadValues_ShouldFailTheDay_OrDropOnlyTheBodyFat()
    {
        var (user, useCase) = await Setup();

        var output = await useCase.Execute(BodyOnly(
            new(Oct1, 900m),
            new(Oct1.AddDays(1), 80m, 95m),
            new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), 80m)));

        Assert.Equal(1, output.Body!.Imported);
        Assert.Equal(1, output.Body.DroppedBodyFat);
        Assert.Equal(2, output.Body.Failures.Count);
        Assert.Equal(Oct1, output.Body.Failures[0].Date);
        var kept = Assert.Single(await BodyMeasurementRepository.ForUser(user.Id));
        Assert.Equal(80m, kept.WeightKg);
        Assert.Null(kept.BodyFatPct);
    }

    [Fact]
    public async Task OlderDaysThanTheLog_ShouldNotChangeCurrentWeight()
    {
        var (user, useCase) = await Setup(77m);
        await BodyMeasurementRepository.Save(BodyMeasurement.Create(user.Id, new DateOnly(2026, 11, 1), 77m, null, BodyMeasurementSource.Manual));

        await useCase.Execute(BodyOnly(new ImportBodyInput(Oct1, 82m)));

        Assert.Equal(77m, (await UserRepository.ById(user.Id)).Value.Weight);
    }

    [Fact]
    public async Task NothingNew_ShouldNotCreateAnImportBatch()
    {
        var (user, useCase) = await Setup();
        await BodyMeasurementRepository.Save(BodyMeasurement.Create(user.Id, Oct1, 79m, null, BodyMeasurementSource.Manual));

        var output = await useCase.Execute(BodyOnly(new ImportBodyInput(Oct1, 80m)));

        Assert.Null(output.BatchId);
        Assert.Empty(await ImportBatchRepository.ForUser(user.Id));
    }
}
