using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuranLogix.Application.Features.Documents.Commands;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Api.Controllers;

[ApiController]
[Route("api/orders/{orderId:int}/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetByOrder(int orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDocumentsByOrderQuery(orderId), cancellationToken);
        if (result.IsFailure)
            return NotFound(new { error = result.Error.Message });
        return Ok(result.Value);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        int orderId,
        IFormFile file,
        [FromForm] string title,
        [FromForm] DocumentType type,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest(new { error = "Файл не может быть пустым" });

        using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(
            orderId, title, type, stream, file.FileName, file.ContentType);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(new { documentId = result.Value });
    }

    [HttpPost("{documentId:int}/sign")]
    public async Task<IActionResult> Sign(int orderId, int documentId, [FromBody] SignRequest request, CancellationToken cancellationToken)
    {
        var command = new SignDocumentCommand(documentId, request.Certificate);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return NoContent();
    }
}

public record SignRequest(string Certificate);
