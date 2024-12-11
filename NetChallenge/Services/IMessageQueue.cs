public interface IMessageQueue
{
    void Publish(string stockCode, string chatId);
}
