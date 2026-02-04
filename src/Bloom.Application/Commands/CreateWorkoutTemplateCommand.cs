namespace Bloom.Application.Commands;

public sealed record CreateWorkoutTemplateCommandData(

) : ICommand;

public class CreateWorkoutTemplateCommand(CreateWorkoutTemplateCommandData input)
{
    
}