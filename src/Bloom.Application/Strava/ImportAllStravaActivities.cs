using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Strava;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Strava;

public sealed record ImportAllStravaActivitiesInput;
public sealed record ImportAllStravaActivitiesOutput(int Imported);

public class ImportAllStravaActivities(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    StravaImportService importService,
    ILogger<ImportAllStravaActivities> logger
) : IUseCase<ImportAllStravaActivitiesInput, ImportAllStravaActivitiesOutput>
{
    public async Task<ImportAllStravaActivitiesOutput> Execute(ImportAllStravaActivitiesInput input, CancellationToken ct = default)
    {
        var connRepo = uow.Repo<IStravaConnectionRepository>();
        var connection = await connRepo.ByUserId(currentUser.UserId, ct);
        if (!connection.HasValue)
            throw new StravaConnectionNotFoundException("Strava is not connected for this user");

        var conn = connection.Value;
        var token = await importService.EnsureValidToken(conn, connRepo, uow, ct);

        logger.LogInformation("Starting bulk Strava import for user {UserId}", currentUser.UserId);

        var result = await importService.ImportLoop(token, afterUnix: null, currentUser.UserId, uow, ct);

        logger.LogInformation("Strava bulk import complete: {Count} activities imported", result.Imported);
        return new ImportAllStravaActivitiesOutput(result.Imported);
    }
}
