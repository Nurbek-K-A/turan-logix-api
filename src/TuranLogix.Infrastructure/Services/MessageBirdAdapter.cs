using MessageBird.Objects;

namespace TuranLogix.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="IMessageBirdAdapter"/> поверх официального SDK <c>MessageBird.Client</c>
/// </summary>
public sealed class MessageBirdAdapter : IMessageBirdAdapter
{
    private readonly MessageBird.Client _client;

    /// <param name="client">Экземпляр MessageBird клиента (Singleton)</param>
    public MessageBirdAdapter(MessageBird.Client client) => _client = client;

    /// <inheritdoc/>
    public void SendSms(string originator, long[] recipients, string body)
    {
        _client.SendMessage(originator, body, recipients, null);
    }

    /// <inheritdoc/>
    public string CreateVerify(string phoneNumber, int tokenLength, int timeoutSeconds)
    {
        var args = new VerifyOptionalArguments
        {
            TokenLength = tokenLength,
            Timeout = timeoutSeconds
        };
        var result = _client.CreateVerify(phoneNumber, args);
        return result.Id;
    }

    /// <inheritdoc/>
    public bool VerifyToken(string verifyId, string token)
    {
        try
        {
            _client.SendVerifyToken(verifyId, token);
            return true;
        }
        catch (MessageBird.Exceptions.ErrorException)
        {
            return false;
        }
    }
}
