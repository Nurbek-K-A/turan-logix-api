using MediatR;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Common.Models;
using TuranLogix.Application.DTOs.Documents;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;
using TuranLogix.Domain.Errors;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Features.Documents.Commands;

public record UploadDocumentCommand(
    int OrderId,
    string Title,
    DocumentType Type,
    Stream FileStream,
    string FileName,
    string ContentType) : IRequest<Result<int>>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<int>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

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

    private static string ComputeSha256(Stream stream)
    {
        stream.Position = 0;
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}

public record SignDocumentCommand(int DocumentId, string Certificate) : IRequest<Result>;

public class SignDocumentCommandHandler : IRequestHandler<SignDocumentCommand, Result>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISignatureService _signatureService;

    public SignDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        ISignatureService signatureService)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _signatureService = signatureService;
    }

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

public record GetDocumentsByOrderQuery(int OrderId) : IRequest<Result<IReadOnlyList<DocumentDto>>>;

public class GetDocumentsByOrderQueryHandler : IRequestHandler<GetDocumentsByOrderQuery, Result<IReadOnlyList<DocumentDto>>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IOrderRepository _orderRepository;

    public GetDocumentsByOrderQueryHandler(IDocumentRepository documentRepository, IOrderRepository orderRepository)
    {
        _documentRepository = documentRepository;
        _orderRepository = orderRepository;
    }

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
