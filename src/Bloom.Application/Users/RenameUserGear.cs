using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record RenameUserGearInput(string From, string To);

/// <param name="Workouts">Logged workouts that used the item and now carry the new name.</param>
/// <param name="Templates">Templates that planned the item and now carry the new name.</param>
public sealed record RenameUserGearOutput(int Workouts, int Templates);

/// <summary>
/// Renames a gear item everywhere: in the user's gear list, on every logged
/// exercise and every template exercise that uses it. Logs refer to gear by
/// name, so renaming only the list would cut the item off from its history
/// (its distance and session counts). All of it is saved in one unit of work.
/// </summary>
public class RenameUserGear(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery logsQuery,
    IFindWorkoutTemplatesQuery templatesQuery,
    ILogger<RenameUserGear> logger
) : IUseCase<RenameUserGearInput, RenameUserGearOutput>
{
    public async Task<RenameUserGearOutput> Execute(RenameUserGearInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        var from = input.From;
        var to = string.Join(' ', (input.To ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        if (to.Length == 0) throw new BloomGeneralException("Give the gear a name.");
        if (to.Length > 100) throw new BloomGeneralException("Gear names are at most 100 characters.");

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);
        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        if (!user.Value.Gear.Contains(from))
            throw new BloomGeneralException($"“{from}” isn't in your gear.");
        if (to == from) return new RenameUserGearOutput(0, 0);
        // A case-only change of the same item is fine; taking another item's name is not.
        if (user.Value.Gear.Any(g => g != from && string.Equals(g, to, StringComparison.OrdinalIgnoreCase)))
            throw new BloomGeneralException($"“{to}” is already in your gear.");

        logger.LogInformation("Renaming gear | Id: {UserId}", userId);
        user.Value.RenameGear(from, to);
        await userRepo.Save(user.Value);

        var uid = userId.Value;
        var workoutRepo = uow.Repo<ILoggedWorkoutRepository>();
        var workoutIds = (await logsQuery.Fetch(l => l.UserId == uid, ct))
            .Where(l => l.LoggedExercises.Any(e => e.Gear.Contains(from)))
            .Select(l => l.Id);
        var workouts = 0;
        foreach (var id in workoutIds)
        {
            var log = await workoutRepo.ById(EntityId.New<LoggedWorkoutId>(id));
            if (!log.HasValue || log.Value.UserId != userId || !log.Value.RenameGear(from, to)) continue;
            await workoutRepo.Save(log.Value);
            workouts++;
        }

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var templateIds = (await templatesQuery.Fetch(t => t.UserId == uid, ct))
            .Where(t => t.TemplateExercises.Any(e => e.Gear.Contains(from)))
            .Select(t => t.Id);
        var templates = 0;
        foreach (var id in templateIds)
        {
            var template = await templateRepo.ById(EntityId.New<WorkoutTemplateId>(id));
            if (!template.HasValue || template.Value.UserId != userId || !template.Value.RenameGear(from, to)) continue;
            await templateRepo.Save(template.Value);
            templates++;
        }

        await uow.Do(ct);
        logger.LogInformation("Gear renamed | Id: {UserId} | Workouts: {Workouts} | Templates: {Templates}", userId, workouts, templates);
        return new RenameUserGearOutput(workouts, templates);
    }
}
