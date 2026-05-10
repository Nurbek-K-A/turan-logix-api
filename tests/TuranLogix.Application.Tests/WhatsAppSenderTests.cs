using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TuranLogix.Infrastructure.Options;
using TuranLogix.Infrastructure.Services.Notifications;

namespace TuranLogix.Application.Tests;

public class WhatsAppSenderTests
{
    private readonly Mock<ILogger<WhatsAppSender>> _loggerMock = new();

    private static IOptions<BirdOptions> CreateOptions(string channelId = "ch-123") =>
        Options.Create(new BirdOptions
        {
            AccessKey = "test-key",
            WhatsApp = new WhatsAppOptions
            {
                ChannelId = channelId,
                Namespace = "test-ns",
                TemplateName = "notification"
            }
        });

    private static IHttpClientFactory CreateFactory(HttpStatusCode status = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(status));

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://conversations.messagebird.com")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("Bird")).Returns(client);
        return factoryMock.Object;
    }

    [Fact]
    public async Task SendAsync_ValidConfig_PostsToConversationsApi()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://conversations.messagebird.com")
        };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("Bird")).Returns(client);

        var sender = new WhatsAppSender(factoryMock.Object, CreateOptions(), _loggerMock.Object);
        await sender.SendAsync("+77001234567", "Тест");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.PathAndQuery.Should().Be("/v1/send");
    }

    [Fact]
    public async Task SendAsync_EmptyChannelId_DoesNotMakeRequest()
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        var sender = new WhatsAppSender(factoryMock.Object, CreateOptions(channelId: ""), _loggerMock.Object);

        await sender.SendAsync("+77001234567", "Test");

        factoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_ApiReturnsError_ThrowsHttpRequestException()
    {
        var factory = CreateFactory(HttpStatusCode.InternalServerError);
        var sender = new WhatsAppSender(factory, CreateOptions(), _loggerMock.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("+77001234567", "Test"));
    }
}
