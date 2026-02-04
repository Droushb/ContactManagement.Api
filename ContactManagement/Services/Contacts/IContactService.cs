using ContactManagement.DTOs;

namespace ContactManagement.Services.Contacts;

public interface IContactService
{
    Task<PagedResultDto<ContactDto>> GetPagedAsync(ContactFilterDto filter, CancellationToken cancellationToken = default);
    Task<ContactDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContactDto> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken = default);
    Task<ContactDto?> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
