using System.Threading.Channels;
namespace CinemaGo.Infrastructure.Notifications;

public sealed class EmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(1000);
    public bool TryEnqueue(EmailMessage message) => _channel.Writer.TryWrite(message);
    public ChannelReader<EmailMessage> Reader => _channel.Reader;
}
