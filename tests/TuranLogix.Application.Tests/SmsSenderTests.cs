using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TuranLogix.Infrastructure.Options;
using TuranLogix.Infrastructure.Services;
using TuranLogix.Infrastructure.Services.Notifications;

namespace TuranLogix.Application.Tests;

public class SmsSenderTests
{
    private readonly Mock<IMessageBirdAdapter> _adapterMock = new();
    private readonly Mock<ILogger<SmsSender>> _loggerMock = new();
    private readonly IOptions<BirdOptions> _options = Options.Create(new BirdOptions { AccessKey = "test-key" });

    private SmsSender CreateSender() => new(_adapterMock.Object, _options, _loggerMock.Object);

    [Fact]
    public async Task SendAsync_ValidPhoneWithPlus_CallsAdapterWithCorrectMsisdn()
    {
        var sender = CreateSender();

        await sender.SendAsync("+77001234567", "Тестовое сообщение");

        _adapterMock.Verify(
            a => a.SendSms("TuranLogix", It.Is<long[]>(r => r.Length == 1 && r[0] == 77001234567L), "Тестовое сообщение"),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ValidPhoneWithoutPlus_CallsAdapterWithCorrectMsisdn()
    {
        var sender = CreateSender();

        await sender.SendAsync("77001234567", "Hello");

        _adapterMock.Verify(
            a => a.SendSms("TuranLogix", It.Is<long[]>(r => r[0] == 77001234567L), "Hello"),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_InvalidPhoneNumber_DoesNotCallAdapter()
    {
        var sender = CreateSender();

        await sender.SendAsync("invalid-phone", "Test");

        _adapterMock.Verify(a => a.SendSms(It.IsAny<string>(), It.IsAny<long[]>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_AdapterThrows_RethrowsException()
    {
        _adapterMock
            .Setup(a => a.SendSms(It.IsAny<string>(), It.IsAny<long[]>(), It.IsAny<string>()))
            .Throws(new Exception("Bird API error"));

        var sender = CreateSender();

        await Assert.ThrowsAsync<Exception>(() => sender.SendAsync("+77001234567", "Test"));
    }
}
