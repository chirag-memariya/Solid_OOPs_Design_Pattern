using Gatherly.Domain.Entities;
using Gatherly.Domain.Repositories;

namespace Gatherly.Persistence.Repository;

internal sealed class GatheringRepository : IGatheringRepository
{
    public Task<Gathering?> GetByIdWithCreatorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Gathering?> GetByIdWithInvitationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Add(Gathering gathering)
    {
        throw new NotImplementedException();
    }

    public void Remove(Gathering gathering)
    {
        throw new NotImplementedException();
    }
}