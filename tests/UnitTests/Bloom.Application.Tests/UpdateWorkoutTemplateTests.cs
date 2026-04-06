using Bloom.Application.Commands;
using Bloom.Application.Common.Behaviours;
using Microsoft.Extensions.Logging;
using UnitTests.Mock;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests.Bloom.Application.Tests;

public class UpdateWorkoutTemplateTests
{
    private readonly MockWorkoutTemplateRepository _mockTemplateRepository;
    private readonly MockExerciseRepository _mockExerciseRepository;
    private readonly MockLogger<UpdateWorkoutTemplateCommandHandler> _mockLogger;
    private readonly UpdateWorkoutTemplateCommandHandler _handler;


    public UpdateWorkoutTemplateTests(MockWorkoutTemplateRepository mockTemplateRepository, MockExerciseRepository mockExerciseRepository, MockLogger<UpdateWorkoutTemplateCommandHandler> mockLogger, UpdateWorkoutTemplateCommandHandler handler)
    {
        _mockTemplateRepository = mockTemplateRepository;
        _mockExerciseRepository = mockExerciseRepository;
        _mockLogger = mockLogger;
        _handler = handler;
    }

    // [Fact]
    // public async Task Handle_WhenUserNotAuthenticated_ReturnsFailure()
    // {
    //     
    // }
}