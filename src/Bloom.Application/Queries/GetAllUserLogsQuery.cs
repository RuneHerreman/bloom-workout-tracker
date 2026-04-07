// using Bloom.Application.Common;
// using Bloom.Application.Common.Behaviours;
// using Bloom.Application.Common.Mappings;
// using Bloom.Application.Contracts.Data.LogBook;
// using Bloom.Domain.LogBook;
// using Bloom.Domain.Users;
// using MediatR;
// using Microsoft.Extensions.Logging;
//
// namespace Bloom.Application.Queries;
//
// public record GetAllUserLogsQuery(): IRequest<Result<List<LoggedWorkoutData>>>;
//
// public class GetAllUserLogsQueryHandler
//     : IRequestHandler<GetAllUserLogsQuery, Result<List<LoggedWorkoutData>>>
// {
//     private readonly ICurrentUserService _currentUserService;
//     private readonly ILogger<GetAllUserTemplatesQueryHandler> _logger;
//     private readonly IUserRepository _userRepository;
//     private readonly ILogBookRepository _logBookRepository;
//
//     public GetAllUserLogsQueryHandler(
//         ICurrentUserService currentUserService, 
//         ILogger<GetAllUserTemplatesQueryHandler> logger, 
//         ILogBookRepository logBookRepository, 
//         IUserRepository userRepository)
//     {
//         _currentUserService = currentUserService;
//         _logger = logger;
//         _logBookRepository = logBookRepository;
//         _userRepository = userRepository;
//     }
//
//     public async Task<Result<List<LoggedWorkoutData>>> Handle(GetAllUserLogsQuery request, CancellationToken cancellationToken)
//     {
//         var userId = _currentUserService.UserId;
//
//         if (!userId.HasValue || await _userRepository.GetUserById(userId.Value, cancellationToken) is null)
//             return Result<List<LoggedWorkoutData>>.Failure("User not authenticated or not found");
//         
//         var workouts = await _logBookRepository.GetAllUserWorkoutsAsync(userId.Value, cancellationToken);
//         _logger.LogInformation("User {id} retrieved all workouts: {workoutsCount}", userId.Value, workouts.Count);
//         
//         return Result<List<LoggedWorkoutData>>.Success(workouts.Select(w => w.ToDtoShort()).ToList());
//     }
// }