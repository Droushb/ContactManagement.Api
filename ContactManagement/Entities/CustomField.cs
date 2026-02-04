namespace ContactManagement.Entities;

public class CustomField
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<ContactCustomFieldValue> ContactValues { get; set; } = new List<ContactCustomFieldValue>();
}
