using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates.DomainEvents;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct WorkoutTemplateId(Guid Value) : IEntityId;

public class WorkoutTemplate: AggregateRoot<WorkoutTemplateId>
{
    private readonly List<TemplateExercise> _templateExercises = [];

    public UserId UserId { get; private set; }
    public WorkoutTemplateName Name { get; private set; }
    public IReadOnlyList<TemplateExercise> TemplateExercises => _templateExercises.AsReadOnly();

    private WorkoutTemplate() { }

    private WorkoutTemplate(
        WorkoutTemplateId id,
        UserId userId,
        WorkoutTemplateName name,
        List<TemplateExercise> templateExercises) : base(id)
    {
        UserId = userId;
        Name = name;
        _templateExercises = templateExercises;
    }

    public static WorkoutTemplate Create(
        UserId userId,
        string name,
        List<TemplateExercise> templateExercises)
    {
        var template = new WorkoutTemplate(
            EntityId.New<WorkoutTemplateId>(),
            userId,
            WorkoutTemplateName.Create(name),
            templateExercises);

        template.ValidateState();
        template.RaiseDomainEvent(new WorkoutTemplateCreated(template.Id));

        return template;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(UserId);
        Asserts.EnsureNotEmpty(Name);
        Asserts.EnsureNotEmpty(_templateExercises);
    }
}