using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Features.Auth.Commands;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Tests;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IBirdVerifyService> _verifyServiceMock = new();
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock = new();

    private RegisterCommandHandler CreateHandler() =>
        new(_userRepositoryMock.Object, _unitOfWorkMock.Object, _passwordHasherMock.Object,
            _verifyServiceMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        var existingUser = User.Create("Тест", "existing@test.kz", "+77001234567", "hash");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("existing@test.kz", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var command = new RegisterCommand("Иван", "existing@test.kz", "+77001234567", "Password123", null, null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");
    }

    [Fact]
    public async Task Handle_WhenNewEmail_ShouldRegisterUserAndSendOtp()
    {
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("new@test.kz", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(h => h.Hash("Password123")).Returns("hashed_pw");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _verifyServiceMock.Setup(v => v.SendOtpAsync("+77001234567", It.IsAny<CancellationToken>()))
            .ReturnsAsync("verify-id-123");

        var command = new RegisterCommand("Иван Иванов", "new@test.kz", "+77001234567", "Password123", null, null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _verifyServiceMock.Verify(v => v.SendOtpAsync("+77001234567", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOtpSendFails_ShouldStillSucceed()
    {
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("new@test.kz", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(h => h.Hash("Password123")).Returns("hashed_pw");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _verifyServiceMock.Setup(v => v.SendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bird API недоступен"));

        var command = new RegisterCommand("Иван Иванов", "new@test.kz", "+77001234567", "Password123", null, null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
