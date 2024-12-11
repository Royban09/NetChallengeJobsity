using Microsoft.AspNetCore.SignalR;
using NetChallenge.Models;
using System.Collections.Concurrent;

public class ChatHub : Hub
{
    private static readonly Chat _defaultChat = new()
    {
        Id = "1",
        Name = "Default Chat"
    };

    public static readonly ConcurrentDictionary<string, List<Message>> Chats = new()
    {
        [_defaultChat.Id] = new List<Message>()
    };

    private static readonly List<Chat> ChatList = new()
    {
        _defaultChat
    };

    private readonly IMessageQueue _messageQueue;

    public ChatHub(IMessageQueue messageQueue)
    {
        _messageQueue = messageQueue;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("UpdateChatList", ChatList);
        await Clients.Caller.SendAsync("SelectDefaultChat");

        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string chatId, string message)
    {
        var user = Context.User.Identity.Name ?? "Anonymous";
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (message.Contains("/stock="))
        {
            var startIndex = message.IndexOf("/stock=") + "/stock=".Length;

            var endIndex = message.IndexOf(' ', startIndex);
            var stockCode = (endIndex == -1)
                ? message[startIndex..]
                : message[startIndex..endIndex];

            _messageQueue.Publish(stockCode, chatId);
        }
        else
        {
            if (Chats.TryGetValue(chatId, out var messages))
            {
                messages.Add(new()
                { 
                    Timestamp = timestamp,
                    User = user,
                    MessageText = message
                });

                if (messages.Count > 50) messages.RemoveAt(0);
            }

            await Clients.Group(chatId).SendAsync("ReceiveMessage", chatId, user, message, timestamp);
        }
    }

    public async Task CreateChat(string chatName)
    {
        var chatId = Guid.NewGuid().ToString();
        Chats.TryAdd(chatId, new List<Message>());
        ChatList.Add(new Chat { Id = chatId, Name = chatName });

        await Clients.All.SendAsync("UpdateChatList", ChatList);
    }

    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task RetrieveMessages(string chatId)
    {
        var messages = Chats.GetValueOrDefault(chatId);
        await Clients.Caller.SendAsync("LoadMessages", messages);
    }
}
