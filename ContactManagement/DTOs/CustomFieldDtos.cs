using ContactManagement.Entities;

namespace ContactManagement.DTOs;

public class CustomFieldDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCustomFieldRequest
{
    public string Name { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
}

public class UpdateCustomFieldRequest
{
    public string Name { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
}
