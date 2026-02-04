using ContactManagement.Data;
using ContactManagement.DTOs;
using ContactManagement.Entities;
using ContactManagement.Services.Contacts;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Services;

public class CustomFieldService : ICustomFieldService
{
    private readonly ContactManagementDbContext _db;

    public CustomFieldService(ContactManagementDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomFieldDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.CustomFields
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
        
        return list.Select(MapToDto).ToList();
    }

    public async Task<CustomFieldDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CustomFields.FindAsync([id], cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<CustomFieldDto> CreateAsync(CreateCustomFieldRequest request, CancellationToken cancellationToken = default)
    {
        ValidateFieldType(request.FieldType);
        var entity = new CustomField
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            FieldType = request.FieldType,
            CreatedAt = DateTime.UtcNow
        };
        _db.CustomFields.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<CustomFieldDto?> UpdateAsync(Guid id, UpdateCustomFieldRequest request, CancellationToken cancellationToken = default)
    {
        ValidateFieldType(request.FieldType);
        var entity = await _db.CustomFields.FindAsync([id], cancellationToken);
        if (entity == null) return null;
        entity.Name = request.Name.Trim();
        entity.FieldType = request.FieldType;
        await _db.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CustomFields.FindAsync([id], cancellationToken);
        if (entity == null) return false;
        _db.CustomFields.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    #region Private Methods
    
    private static void ValidateFieldType(CustomFieldType fieldType)
    {
        if (!Enum.IsDefined(typeof(CustomFieldType), fieldType))
            throw new ArgumentException("Invalid FieldType. Must be String, Int, or Bool.");
    }

    private static CustomFieldDto MapToDto(CustomField f)
    {
        return new CustomFieldDto
        {
            Id = f.Id,
            Name = f.Name,
            FieldType = f.FieldType,
            CreatedAt = f.CreatedAt
        };
    }
    
    #endregion
}
