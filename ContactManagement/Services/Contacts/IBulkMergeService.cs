using ContactManagement.DTOs;

namespace ContactManagement.Services.Contacts;

public interface IBulkMergeService
{
    Task<BulkMergeResultDto> MergeByContactIdsAsync(List<Guid> contactIds, CancellationToken cancellationToken = default);
}
