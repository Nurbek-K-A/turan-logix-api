using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Features.Phone.Commands;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Tests;

// ───────────────────────────────────────────────────────────────────────────
// SendPhoneOtpCommandHandler tests
// ───────────────────────────────────────────────────────────────────────────

public class SendPhoneOtpCommandHandlerTests
{
    private readonly Mock<IBirdVerifyService> _verifyServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<SendPhoneOtpCommandHandler>> _loggerMock = new();

    private SendPhoneOtpCommandHandler CreateHandler() =>
        new(_verifyServiceMock.Object, _userRepositoryMock.Object,
            _unitOfWorkMock.Object, _loggerMock.Object);

    private static User CreateUser(string phone) =>
        User.Create("Иван Иванов", "ivan@test.kz", phone, "hashedpw");

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new SendPhoneOtpCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
        _verifyServiceMock.Verify(v => v.SendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPhoneNumberEmpty_ShouldReturnFailureWithoutCallingBird()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "", "hashedpw");
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateHandler().Handle(new SendPhoneOtpCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Phone.PhoneNotSet");
        _verifyServiceMock.Verify(v => v.SendOtpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidPhone_ShouldCallBirdWithProfilePhoneAndSavePendingState()
    {
        var user = CreateUser("+77001234567");
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _verifyServiceMock.Setup(v => v.TimeoutSeconds).Returns(300);
        _verifyServiceMock
            .Setup(v => v.SendOtpAsync("+77001234567", It.IsAny<CancellationToken>()))
            .ReturnsAsync("verify-xyz");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await CreateHandler().Handle(new SendPhoneOtpCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("verify-xyz");

        // Bird called with profile phone, not with any client-supplied value
        _verifyServiceMock.Verify(v => v.SendOtpAsync("+77001234567", It.IsAny<CancellationToken>()), Times.Once);

        // Pending state must be saved
        user.PendingVerifyId.Should().Be("verify-xyz");
        user.PendingVerifyPhone.Should().Be("+77001234567");
        user.PendingVerifyExpiresAt.Should().NotBeNull();

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

// ───────────────────────────────────────────────────────────────────────────
// ConfirmPhoneOtpCommandHandler tests
// ───────────────────────────────────────────────────────────────────────────

public class ConfirmPhoneOtpCommandHandlerTests
{
    private readonly Mock<IBirdVerifyService> _verifyServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<ConfirmPhoneOtpCommandHandler>> _loggerMock = new();

    private ConfirmPhoneOtpCommandHandler CreateHandler() =>
        new(_verifyServiceMock.Object, _userRepositoryMock.Object,
            _unitOfWorkMock.Object, _loggerMock.Object);

    private static User CreateUserWithPending(string phone, string verifyId, int secondsUntilExpiry = 300)
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", phone, "hashedpw");
        user.SetPendingVerification(verifyId, phone, secondsUntilExpiry);
        return user;
    }

    [Fact]
    public async Task Handle_WhenVerifyIdMismatch_ShouldReturnFailureAndLeavePhoneUnverified()
    {
        var user = CreateUserWithPending("+77001111111", "verify-correct");
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new ConfirmPhoneOtpCommand("verify-WRONG", "1234", 1);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Phone.InvalidVerifySession");
        user.IsPhoneVerified.Should().BeFalse();
        _verifyServiceMock.Verify(v => v.VerifyOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPhoneChangedAfterOtpSent_ShouldReturnFailureAndLeavePhoneUnverified()
    {
        // PendingVerifyPhone stores the old number, but user.PhoneNumber is now different
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77009999999", "hashedpw");
        user.SetPendingVerification("verify-abc", "+77001111111", 300); // mismatch intentional

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new ConfirmPhoneOtpCommand("verify-abc", "1234", 1);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Phone.PhoneChangedAfterOtp");
        user.IsPhoneVerified.Should().BeFalse();
        _verifyServiceMock.Verify(v => v.VerifyOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOtpSessionExpired_ShouldReturnFailureAndLeavePhoneUnverified()
    {
        // Pass -1 seconds → already expired
        var user = CreateUserWithPending("+77001111111", "verify-abc", -1);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new ConfirmPhoneOtpCommand("verify-abc", "1234", 1);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Phone.OtpSessionExpired");
        user.IsPhoneVerified.Should().BeFalse();
        _verifyServiceMock.Verify(v => v.VerifyOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAllChecksPassAndBirdReturnsTrue_ShouldVerifyPhoneAndClearPending()
    {
        var user = CreateUserWithPending("+77001111111", "verify-abc", 300);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _verifyServiceMock
            .Setup(v => v.VerifyOtpAsync("verify-abc", "1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ConfirmPhoneOtpCommand("verify-abc", "1234", 1);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        user.IsPhoneVerified.Should().BeTrue();
        user.PendingVerifyId.Should().BeNull();
        user.PendingVerifyPhone.Should().BeNull();
        user.PendingVerifyExpiresAt.Should().BeNull();

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBirdReturnsFalse_ShouldReturnFailureAndLeavePhoneUnverified()
    {
        var user = CreateUserWithPending("+77001111111", "verify-abc", 300);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _verifyServiceMock
            .Setup(v => v.VerifyOtpAsync("verify-abc", "9999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new ConfirmPhoneOtpCommand("verify-abc", "9999", 1);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Phone.OtpVerificationFailed");
        user.IsPhoneVerified.Should().BeFalse();
    }
}
