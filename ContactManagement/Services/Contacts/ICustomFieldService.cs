using ContactManagement.DTOs;

namespace ContactManagement.Services.Contacts;

public interface ICustomFieldService
{
    Task<List<CustomFieldDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomFieldDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomFieldDto> CreateAsync(CreateCustomFieldRequest request, CancellationToken cancellationToken = default);
    Task<CustomFieldDto?> UpdateAsync(Guid id, UpdateCustomFieldRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
