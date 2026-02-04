using ContactManagement.DTOs;
using ContactManagement.Services.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly IBulkMergeService _bulkMergeService;

    public ContactsController(IContactService contactService, IBulkMergeService bulkMergeService)
    {
        _contactService = contactService;
        _bulkMergeService = bulkMergeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ContactDto>>> GetPaged(
        [FromQuery] ContactFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _contactService.GetPagedAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContactDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var contact = await _contactService.GetByIdAsync(id, cancellationToken);
        if (contact == null)
            return NotFound();
        return Ok(contact);
    }

    [HttpPost]
    public async Task<ActionResult<ContactDto>> Create([FromBody] CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var contact = await _contactService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("email"))
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContactDto>> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        var contact = await _contactService.UpdateAsync(id, request, cancellationToken);
        if (contact == null)
            return NotFound();
        return Ok(contact);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _contactService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    [HttpPost("merge")]
    public async Task<ActionResult<BulkMergeResultDto>> Merge([FromBody] BulkMergeRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _bulkMergeService.MergeByContactIdsAsync(request.ContactIds ?? new List<Guid>(), cancellationToken);
        return Ok(result);
    }
}
