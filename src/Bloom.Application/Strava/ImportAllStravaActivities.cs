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
    IStravaActivityImporter importer,
    ILogger<ImportAllStravaActivities> logger
) : IUseCase<ImportAllStravaActivitiesInput, ImportAllStravaActivitiesOutput>
{
    public async Task<ImportAllStravaActivitiesOutput> Execute(ImportAllStravaActivitiesInput input, CancellationToken ct = default)
    {
        var connection = await uow.Repo<IStravaConnectionRepository>().ByUserId(currentUser.UserId, ct);

        if (!connection.HasValue)
            throw new StravaConnectionNotFoundException("Strava is not connected for this user");

        logger.LogInformation("Starting bulk Strava import for user {UserId}", currentUser.UserId);

        var imported = await importer.ImportAll(connection.Value, currentUser.UserId, after: null, ct);

        logger.LogInformation("Strava bulk import complete: {Count} activities imported", imported);

        return new ImportAllStravaActivitiesOutput(imported);
    }
}
