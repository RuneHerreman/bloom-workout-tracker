// using Bloom.Application.Common;
// using Bloom.Application.Common.Behaviours;
// using Bloom.Domain.Templates;
// using Bloom.Domain.Users;
// using Bloom.Infrastructure.Persistence;
// using MediatR;
// using Microsoft.Extensions.Logging;
//
// namespace Bloom.Application.Commands;
//
// public record DeleteTemplateComand(
//     Guid Id
// ) : IRequest<Result>;
//
// public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateComand, Result>
// {
//     private readonly IWorkoutTemplateRepository _templateRepository;
//     private readonly IUserRepository _userRepository;
//     private readonly ICurrentUserService _currentUserService;
//     private readonly ILogger<DeleteTemplateCommandHandler> _logger;
//
//     public DeleteTemplateCommandHandler(
//         IWorkoutTemplateRepository templateRepository, 
//         ILogger<DeleteTemplateCommandHandler> logger, 
//         ICurrentUserService currentUserService, 
//         IUserRepository userRepository)
//     {
//         _templateRepository = templateRepository;
//         _logger = logger;
//         _currentUserService = currentUserService;
//         _userRepository = userRepository;
//     }
//
//     public async Task<Result> Handle(
//         DeleteTemplateComand request, 
//         CancellationToken ct)
//     {
//         try
//         {
//             var userId = _currentUserService.UserId;
//             if (!userId.HasValue || await _userRepository.GetUserById(userId.Value, ct) is null)
//                 return Result.Failure("User not authenticated or not found");
//
//             _logger.LogInformation("User {id} is deleting workout template {templateId}.", userId.Value, request.Id);
//             var template = await _templateRepository.GetWorkoutTemplateById(request.Id, userId.Value);
//             if (template is null)
//                 return Result.Failure("Template not found");
//
//             await _templateRepository.DeleteWorkoutTemplate(template);
//         
//             _logger.LogInformation("Workout template deleted: {template}", template.Id);
//         
//             return Result.Success();
//         }
//         catch (Exception e)
//         {
//             return Result.Failure(e.Message);
//         }
//
//     }
// }