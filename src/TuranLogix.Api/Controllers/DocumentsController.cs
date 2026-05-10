using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuranLogix.Application.DTOs.Documents;
using TuranLogix.Application.Features.Documents.Commands;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Api.Controllers;

/// <summary>
/// Управление документами, прикреплёнными к заявке на перевозку
/// </summary>
[ApiController]
[Route("api/orders/{orderId:int}/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <param name="mediator">MediatR</param>
    public DocumentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Получить список документов по заявке
    /// </summary>
    /// <param name="orderId">Id заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список документов заявки</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Документы заявки", Description = "Возвращает все документы по Id заявки")]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrder(int orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDocumentsByOrderQuery(orderId), cancellationToken);
        if (result.IsFailure)
            return NotFound(new { error = result.Error.Message });
        return Ok(result.Value);
    }

    /// <summary>
    /// Загрузить документ к заявке
    /// </summary>
    /// <param name="orderId">Id заявки</param>
    /// <param name="file">Файл документа</param>
    /// <param name="title">Название документа</param>
    /// <param name="type">Тип документа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Id загруженного документа</returns>
    [HttpPost("upload")]
    [SwaggerOperation(Summary = "Загрузить документ", Description = "Загружает файл в Azure Blob Storage и сохраняет SHA-256 хэш")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Подписать документ ЭЦП через NCALayer
    /// </summary>
    /// <param name="orderId">Id заявки</param>
    /// <param name="documentId">Id документа</param>
    /// <param name="request">Сертификат ЭЦП в Base64</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpPost("{documentId:int}/sign")]
    [SwaggerOperation(Summary = "Подписать документ ЭЦП", Description = "Подписывает документ сертификатом НУЦ РК через NCALayer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Sign(int orderId, int documentId, [FromBody] SignRequest request, CancellationToken cancellationToken)
    {
        var command = new SignDocumentCommand(documentId, request.Certificate);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return NoContent();
    }
}

/// <summary>
/// Запрос на подписание документа
/// </summary>
/// <param name="Certificate">Сертификат ЭЦП в формате Base64</param>
public record SignRequest(string Certificate);
