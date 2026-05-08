using Bloom.Application.Contracts;
using Bloom.Application.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.WorkoutTemplates;

public sealed class FindWorkoutTemplateByIdTests
{
    [Fact]
    public async Task Execute_WithExistingId_ShouldReturnTemplate()
    {
        Guid id = Guid.NewGuid();
        var data = new List<WorkoutTemplateData>
        {
            new() { Id = id, UserId = Guid.NewGuid(), Name = "Push Day" }
        };
        var useCase = new FindWorkoutTemplateById(new MockFindWorkoutTemplatesQuery(data));

        var output = await useCase.Execute(new FindWorkoutTemplateByIdInput(id));

        Assert.Equal(id, output.Template.Id);
    }

    [Fact]
    public async Task Execute_WithMissingId_ShouldThrow()
    {
        var useCase = new FindWorkoutTemplateById(new MockFindWorkoutTemplatesQuery([]));

        await Assert.ThrowsAsync<WorkoutTemplateNotFoundException>(
            () => useCase.Execute(new FindWorkoutTemplateByIdInput(Guid.NewGuid())));
    }
}
