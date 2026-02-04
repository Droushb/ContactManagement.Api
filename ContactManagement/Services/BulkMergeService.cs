using ContactManagement.Data;
using ContactManagement.DTOs;
using ContactManagement.Entities;
using ContactManagement.Services.Contacts;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Services;

public class BulkMergeService : IBulkMergeService
{
    private readonly ContactManagementDbContext _db;

    public BulkMergeService(ContactManagementDbContext db)
    {
        _db = db;
    }

    public async Task<BulkMergeResultDto> MergeByContactIdsAsync(List<Guid> contactIds, CancellationToken cancellationToken = default)
    {
        if (contactIds == null || contactIds.Count == 0)
            return new BulkMergeResultDto();

        var contacts = await _db.Contacts
            .Include(c => c.CustomFieldValues)
            .Where(c => contactIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var byEmail = contacts
            .GroupBy(c => c.Email.Trim().ToLowerInvariant())
            .Where(g => g.Count() > 1)
            .ToList();

        var masterIds = new List<Guid>();
        var mergedCountByEmail = new Dictionary<string, int>();

        foreach (var group in byEmail)
        {
            var list = group.OrderBy(c => c.CreatedAt).ToList();
            var master = list[0];
            var toMerge = list.Skip(1).ToList();

            var masterCustomFieldIds = new HashSet<Guid>(master.CustomFieldValues.Select(v => v.CustomFieldId));

            foreach (var other in toMerge)
            {
                if (!string.IsNullOrWhiteSpace(other.FirstName)) master.FirstName = other.FirstName;
                if (!string.IsNullOrWhiteSpace(other.LastName)) master.LastName = other.LastName;
                if (!string.IsNullOrWhiteSpace(other.Phone)) master.Phone = other.Phone;
                master.UpdatedAt = DateTime.UtcNow;

                foreach (var val in other.CustomFieldValues)
                {
                    if (!masterCustomFieldIds.Add(val.CustomFieldId))
                        continue;
                    _db.ContactCustomFieldValues.Add(new ContactCustomFieldValue
                    {
                        Id = Guid.NewGuid(),
                        ContactId = master.Id,
                        CustomFieldId = val.CustomFieldId,
                        StringValue = val.StringValue,
                        IntValue = val.IntValue,
                        BoolValue = val.BoolValue
                    });
                }

                _db.Contacts.Remove(other);
            }

            masterIds.Add(master.Id);
            mergedCountByEmail[group.Key] = list.Count;
        }

        if (masterIds.Count == 0)
            return new BulkMergeResultDto();

        await _db.SaveChangesAsync(cancellationToken);

        var mergedEntities = await _db.Contacts
            .Where(c => masterIds.Contains(c.Id))
            .Include(c => c.CustomFieldValues)
            .ThenInclude(v => v.CustomField)
            .ToListAsync(cancellationToken);

        var byId = mergedEntities.ToDictionary(c => c.Id);
        var mergedContacts = masterIds.Select(id => byId[id]).Select(MapToDto).ToList();

        return new BulkMergeResultDto
        {
            MergedContacts = mergedContacts,
            MergedCountByEmail = mergedCountByEmail
        };
    }

    #region Private Methods

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
