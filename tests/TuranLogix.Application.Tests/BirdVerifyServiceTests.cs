using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TuranLogix.Infrastructure.Options;
using TuranLogix.Infrastructure.Services;

namespace TuranLogix.Application.Tests;

public class BirdVerifyServiceTests
{
    private readonly Mock<IMessageBirdAdapter> _adapterMock = new();
    private readonly Mock<ILogger<BirdVerifyService>> _loggerMock = new();

    private static IOptions<BirdOptions> CreateOptions(int tokenLength = 4, int timeout = 300) =>
        Options.Create(new BirdOptions
        {
            Verify = new VerifyOptions { TokenLength = tokenLength, TimeoutSeconds = timeout }
        });

    private BirdVerifyService CreateService() =>
        new(_adapterMock.Object, CreateOptions(), _loggerMock.Object);

    [Fact]
    public async Task SendOtpAsync_ValidPhone_ReturnsVerifyId()
    {
        _adapterMock
            .Setup(a => a.CreateVerify("77001234567", 4, 300))
            .Returns("verify-abc-123");

        var service = CreateService();
        var result = await service.SendOtpAsync("+77001234567");

        result.Should().Be("verify-abc-123");
        _adapterMock.Verify(a => a.CreateVerify("77001234567", 4, 300), Times.Once);
    }

    [Fact]
    public async Task SendOtpAsync_AdapterThrows_ThrowsInvalidOperationException()
    {
        _adapterMock
            .Setup(a => a.CreateVerify(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Throws(new Exception("Bird unavailable"));

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendOtpAsync("+77001234567"));
    }

    [Fact]
    public async Task VerifyOtpAsync_CorrectToken_ReturnsTrue()
    {
        _adapterMock
            .Setup(a => a.VerifyToken("verify-abc-123", "1234"))
            .Returns(true);

        var service = CreateService();
        var result = await service.VerifyOtpAsync("verify-abc-123", "1234");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtpAsync_WrongToken_ReturnsFalse()
    {
        _adapterMock
            .Setup(a => a.VerifyToken("verify-abc-123", "9999"))
            .Returns(false);

        var service = CreateService();
        var result = await service.VerifyOtpAsync("verify-abc-123", "9999");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtpAsync_AdapterThrows_ReturnsFalse()
    {
        _adapterMock
            .Setup(a => a.VerifyToken(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Network error"));

        var service = CreateService();
        var result = await service.VerifyOtpAsync("verify-abc-123", "1234");

        result.Should().BeFalse();
    }
}
