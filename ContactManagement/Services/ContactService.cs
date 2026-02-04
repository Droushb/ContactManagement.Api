using ContactManagement.Data;
using ContactManagement.DTOs;
using ContactManagement.Entities;
using ContactManagement.Services.Contacts;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Services;

public class ContactService : IContactService
{
    private readonly ContactManagementDbContext _db;

    public ContactService(ContactManagementDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<ContactDto>> GetPagedAsync(ContactFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _db.Contacts
            .AsNoTracking()
            .Include(c => c.CustomFieldValues)
            .ThenInclude(v => v.CustomField)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.FirstName))
            query = query.Where(c => c.FirstName.Contains(filter.FirstName));
        
        if (!string.IsNullOrWhiteSpace(filter.LastName))
            query = query.Where(c => c.LastName.Contains(filter.LastName));
        
        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(c => c.Email.Contains(filter.Email));

        var totalCount = await query.CountAsync(cancellationToken);

        var isDesc = string.Equals(filter.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "firstname" => isDesc ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
            "lastname" => isDesc ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName),
            "email" => isDesc ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "updatedat" => isDesc ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt),
            _ => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ContactDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contact = await _db.Contacts
            .AsNoTracking()
            .Include(c => c.CustomFieldValues)
            .ThenInclude(v => v.CustomField)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return contact == null ? null : MapToDto(contact);
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var isContactExists = await _db.Contacts.AnyAsync(c => c.Email.ToLower() == normalizedEmail, cancellationToken);
        if (isContactExists)
            throw new InvalidOperationException("A contact with this email already exists.");

        var now = DateTime.UtcNow;
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Contacts.Add(contact);

        if (request.CustomFieldValues != null && request.CustomFieldValues.Count > 0)
        {
            await ApplyCustomFieldValuesAsync(contact.Id, request.CustomFieldValues, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(contact.Id, cancellationToken)
               ?? throw new InvalidOperationException("Failed to load created contact.");
    }

    public async Task<ContactDto?> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        var contact = await _db.Contacts
            .Include(c => c.CustomFieldValues)
            .ThenInclude(v => v.CustomField)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        
        if (contact == null) return null;

        contact.FirstName = request.FirstName.Trim();
        contact.LastName = request.LastName.Trim();
        contact.Phone = request.Phone?.Trim();
        contact.UpdatedAt = DateTime.UtcNow;

        if (request.CustomFieldValues != null)
        {
            _db.ContactCustomFieldValues.RemoveRange(contact.CustomFieldValues);
            await ApplyCustomFieldValuesAsync(contact.Id, request.CustomFieldValues, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contact = await _db.Contacts.FindAsync([id], cancellationToken);
        
        if (contact == null) return false;
        
        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    #region Private Methods
    
    private async Task ApplyCustomFieldValuesAsync(Guid contactId, List<CustomFieldValueInputDto> inputs, CancellationToken cancellationToken)
    {
        foreach (var input in inputs)
        {
            var customField = await _db.CustomFields.FindAsync([input.CustomFieldId], cancellationToken);
            if (customField == null) continue;

            var value = new ContactCustomFieldValue
            {
                Id = Guid.NewGuid(),
                ContactId = contactId,
                CustomFieldId = customField.Id,
                StringValue = null,
                IntValue = null,
                BoolValue = null
            };
            switch (customField.FieldType)
            {
                case CustomFieldType.String:
                    value.StringValue = input.StringValue;
                    break;
                case CustomFieldType.Int:
                    value.IntValue = input.IntValue;
                    break;
                case CustomFieldType.Bool:
                    value.BoolValue = input.BoolValue;
                    break;
            }
            _db.ContactCustomFieldValues.Add(value);
        }
    }

    private static ContactDto MapToDto(Contact c)
    {
        return new ContactDto
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            CustomFieldValues = c.CustomFieldValues.Select(v => new CustomFieldValueDto
            {
                CustomFieldId = v.CustomFieldId,
                CustomFieldName = v.CustomField?.Name,
                StringValue = v.StringValue,
                IntValue = v.IntValue,
                BoolValue = v.BoolValue
            }).ToList()
        };
    }
    
    #endregion
}
