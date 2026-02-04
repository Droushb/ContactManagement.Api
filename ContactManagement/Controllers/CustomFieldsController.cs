using ContactManagement.DTOs;
using ContactManagement.Services.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomFieldsController : ControllerBase
{
    private readonly ICustomFieldService _customFieldService;

    public CustomFieldsController(ICustomFieldService customFieldService)
    {
        _customFieldService = customFieldService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomFieldDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var list = await _customFieldService.GetAllAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomFieldDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var field = await _customFieldService.GetByIdAsync(id, cancellationToken);
        if (field == null)
            return NotFound();
        return Ok(field);
    }

    [HttpPost]
    public async Task<ActionResult<CustomFieldDto>> Create([FromBody] CreateCustomFieldRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var field = await _customFieldService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = field.Id }, field);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomFieldDto>> Update(Guid id, [FromBody] UpdateCustomFieldRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var field = await _customFieldService.UpdateAsync(id, request, cancellationToken);
            if (field == null)
                return NotFound();
            return Ok(field);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _customFieldService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
