namespace ContactManagement.DTOs;

public class CustomFieldValueDto
{
    public Guid CustomFieldId { get; set; }
    public string? CustomFieldName { get; set; }
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public bool? BoolValue { get; set; }
}

public class CustomFieldValueInputDto
{
    public Guid CustomFieldId { get; set; }
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public bool? BoolValue { get; set; }
}
