using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonnelManager.Api.Auth;
using PersonnelManager.Api.Contracts;
using PersonnelManager.Api.Extensions;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Api.Controllers;

/// <summary>
/// REST endpoints for the Personnel resource. A thin HTTP layer over <see cref="IPersonnelService"/>:
/// it binds/validates requests, maps <see cref="Result{T}"/> to status codes, and enforces authorization.
/// All endpoints require a valid JWT; destructive operations require the Admin role.
/// </summary>
[ApiController]
[Route("api/personnel")]
[Authorize(Roles = $"{Roles.Admin},{Roles.User}")]
[Produces("application/json")]
public sealed class PersonnelController(IPersonnelService service, IPersonnelBackup backup) : ControllerBase
{
    /// <summary>List personnel, optionally filtered by status and/or a name fragment.</summary>
    /// <param name="status">Restrict to a single employment status.</param>
    /// <param name="name">Case-insensitive fragment matched against name or surname.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PersonalDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonalDto>>> GetAll(
        [FromQuery] EmploymentStatus? status,
        [FromQuery] string? name,
        CancellationToken cancellationToken)
    {
        var people = await service.GetAllAsync(cancellationToken);

        IEnumerable<PersonalDto> filtered = people;
        if (status is not null)
            filtered = filtered.Where(person => person.Status == status);
        if (!string.IsNullOrWhiteSpace(name))
            filtered = filtered.Where(person =>
                Contains(person.Name, name) || Contains(person.Surname, name));

        return Ok(filtered.ToList());
    }

    /// <summary>Fetch a single person by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PersonalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonalDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result.ToOk(this);
    }

    /// <summary>Create a new person.</summary>
    [HttpPost]
    [ProducesResponseType<PersonalDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonalDto>> Create(
        [FromBody] CreatePersonRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(
            new CreatePersonalRequest(request.Name, request.Surname, request.Address, request.Phone, request.Status),
            cancellationToken);

        return result.ToActionResult<PersonalDto, PersonalDto>(
            this,
            dto => CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto));
    }

    /// <summary>Replace the mutable fields of an existing person.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<PersonalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonalDto>> Update(
        Guid id, [FromBody] UpdatePersonRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(
            new UpdatePersonalRequest(id, request.Name, request.Surname, request.Address, request.Phone, request.Status),
            cancellationToken);

        return result.ToOk(this);
    }

    /// <summary>Change only a person's employment status (Active / OnLeave / Terminated).</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<PersonalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonalDto>> ChangeStatus(
        Guid id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var current = await service.GetByIdAsync(id, cancellationToken);
        if (!current.IsSuccess)
            return current.ToOk(this);

        var person = current.Value!;
        var result = await service.UpdateAsync(
            new UpdatePersonalRequest(id, person.Name, person.Surname, person.Address, person.Phone, request.Status),
            cancellationToken);

        return result.ToOk(this);
    }

    /// <summary>Delete a person. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error);
    }

    /// <summary>Export all personnel to the configured JSON backup file. Admin only.</summary>
    [HttpPost("backup")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Backup(CancellationToken cancellationToken)
    {
        await backup.SaveAsync(cancellationToken);
        return Accepted(new { status = "saved" });
    }

    /// <summary>Restore personnel from the configured JSON backup file. Admin only.</summary>
    [HttpPost("restore")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Restore(CancellationToken cancellationToken)
    {
        var restored = await backup.RestoreAsync(cancellationToken);
        return Ok(new { restored });
    }

    private static bool Contains(string? value, string fragment) =>
        value is not null && value.Contains(fragment, StringComparison.OrdinalIgnoreCase);
}
