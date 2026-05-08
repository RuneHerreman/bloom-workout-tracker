using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Domain.WorkoutTemplates.DomainEvents;

namespace UnitTests.Domain.WorkoutTemplates.DomainEvents;

public sealed class WorkoutTemplateDomainEventsTests
{
    [Fact]
    public void WorkoutTemplateCreated_ShouldExposeIdAndFqdn()
    {
        WorkoutTemplateId id = EntityId.New<WorkoutTemplateId>();

        WorkoutTemplateCreated evt = new(id);

        Assert.Equal(id.Value.ToString(), evt.WorkoutTemplateId);
        Assert.Equal("Bloom.Bloom.WorkoutTemplate.WorkoutTemplateCreated", evt.FQDN);
    }

    [Fact]
    public void WorkoutTemplateUpdated_ShouldExposeIdAndFqdn()
    {
        WorkoutTemplateId id = EntityId.New<WorkoutTemplateId>();

        WorkoutTemplateUpdated evt = new(id);

        Assert.Equal(id.Value.ToString(), evt.WorkoutTemplateId);
        Assert.Equal("Bloom.Bloom.WorkoutTemplate.WorkoutTemplateUpdated", evt.FQDN);
    }

    [Fact]
    public void WorkoutTemplateDeleted_ShouldExposeIdAndFqdn()
    {
        WorkoutTemplateId id = EntityId.New<WorkoutTemplateId>();

        WorkoutTemplateDeleted evt = new(id);

        Assert.Equal(id.Value.ToString(), evt.WorkoutTemplateId);
        Assert.Equal("Bloom.Bloom.WorkoutTemplate.WorkoutTemplateDeleted", evt.FQDN);
    }

    [Fact]
    public void ExerciseAddedToTemplate_CanBeInstantiated()
    {
        ExerciseAddedToTemplate evt = new();
        Assert.NotNull(evt);
    }

    [Fact]
    public void ExerciseRemovedFromTemplate_CanBeInstantiated()
    {
        ExerciseRemovedFromTemplate evt = new();
        Assert.NotNull(evt);
    }
}
