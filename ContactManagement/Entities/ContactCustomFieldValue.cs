namespace ContactManagement.Entities;

public class ContactCustomFieldValue
{
    public Guid Id { get; set; }
    public Guid ContactId { get; set; }
    public Guid CustomFieldId { get; set; }
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public bool? BoolValue { get; set; }

    public Contact Contact { get; set; } = null!;
    public CustomField CustomField { get; set; } = null!;
}
