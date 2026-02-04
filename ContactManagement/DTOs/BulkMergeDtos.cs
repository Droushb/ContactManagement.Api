namespace ContactManagement.DTOs;

public class BulkMergeRequest
{
    public List<Guid> ContactIds { get; set; } = new();
}

public class BulkMergeResultDto
{
    public List<ContactDto> MergedContacts { get; set; } = new();
    public Dictionary<string, int> MergedCountByEmail { get; set; } = new();
}
