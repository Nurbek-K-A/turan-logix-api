using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Documents;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Documents.Commands;

/// <summary>
/// Команда загрузки документа к заявке
/// </summary>
public record UploadDocumentCommand(
    int OrderId,
    string Title,
    DocumentType Type,
    Stream FileStream,
    string FileName,
    string ContentType) : IRequest<Result<int>>;

/// <summary>
/// Обработчик команды <see cref="UploadDocumentCommand"/>
/// </summary>
public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<int>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    /// <param name="documentRepository">Репозиторий документов</param>
    /// <param name="orderRepository">Репозиторий заявок</param>
    /// <param name="unitOfWork">Единица работы</param>
    /// <param name="fileStorageService">Сервис хранения файлов</param>
    /// <param name="currentUserService">Сервис текущего пользователя</param>
    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Загрузить файл в хранилище, вычислить SHA-256 и сохранить документ
    /// </summary>
    /// <param name="request">Данные документа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Id созданного документа или ошибка Order.NotFound</returns>
    public async Task<Result<int>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<int>(DomainErrors.Order.NotFound);

        var fileUrl = await _fileStorageService.UploadAsync(
            request.FileStream, request.FileName, request.ContentType, cancellationToken);

        var fileHash = ComputeSha256(request.FileStream);

        var document = Document.Create(
            request.Title,
            request.Type,
            fileUrl,
            fileHash,
            request.OrderId,
            _currentUserService.UserId!.Value);

        await _documentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(document.Id);
    }

    /// <summary>
    /// Вычислить SHA-256 хэш потока файла
    /// </summary>
    /// <param name="stream">Поток файла (позиция сбрасывается в 0)</param>
    /// <returns>Hex-строка хэша в нижнем регистре</returns>
    private static string ComputeSha256(Stream stream)
    {
        stream.Position = 0;
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}

/// <summary>
/// Команда подписания документа ЭЦП через NCALayer
/// </summary>
public record SignDocumentCommand(int DocumentId, string Certificate) : IRequest<Result>;

/// <summary>
/// Обработчик команды <see cref="SignDocumentCommand"/>
/// </summary>
public class SignDocumentCommandHandler : IRequestHandler<SignDocumentCommand, Result>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISignatureService _signatureService;

    /// <param name="documentRepository">Репозиторий документов</param>
    /// <param name="unitOfWork">Единица работы</param>
    /// <param name="signatureService">Сервис ЭЦП-подписи</param>
    public SignDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        ISignatureService signatureService)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _signatureService = signatureService;
    }

    /// <summary>
    /// Подписать документ и сохранить CMS-данные подписи
    /// </summary>
    /// <param name="request">Id документа и сертификат ЭЦП</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Успех или ошибка Document.NotFound / Document.AlreadySigned</returns>
    public async Task<Result> Handle(SignDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure(DomainErrors.Document.NotFound);

        if (document.IsSigned)
            return Result.Failure(DomainErrors.Document.AlreadySigned);

        var fileHash = document.FileHash ?? string.Empty;
        var signatureData = await _signatureService.SignDocumentAsync(fileHash, request.Certificate, cancellationToken);

        document.Sign(signatureData);
        _documentRepository.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Запрос списка документов по Id заявки
/// </summary>
public record GetDocumentsByOrderQuery(int OrderId) : IRequest<Result<IReadOnlyList<DocumentDto>>>;

/// <summary>
/// Обработчик запроса <see cref="GetDocumentsByOrderQuery"/>
/// </summary>
public class GetDocumentsByOrderQueryHandler : IRequestHandler<GetDocumentsByOrderQuery, Result<IReadOnlyList<DocumentDto>>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IOrderRepository _orderRepository;

    /// <param name="documentRepository">Репозиторий документов</param>
    /// <param name="orderRepository">Репозиторий заявок</param>
    public GetDocumentsByOrderQueryHandler(IDocumentRepository documentRepository, IOrderRepository orderRepository)
    {
        _documentRepository = documentRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Получить все документы заявки
    /// </summary>
    /// <param name="request">Id заявки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список документов или ошибка Order.NotFound</returns>
    public async Task<Result<IReadOnlyList<DocumentDto>>> Handle(GetDocumentsByOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<IReadOnlyList<DocumentDto>>(DomainErrors.Order.NotFound);

        var documents = await _documentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        var dtos = documents.Select(d => new DocumentDto(
            d.Id, d.Title, d.Type, d.FileUrl, d.FileHash,
            d.OrderId, d.UploadedByUserId, d.IsSigned, d.SignedAt, d.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<DocumentDto>>(dtos);
    }
}
