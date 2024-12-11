using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestNetChallenge
{
    public class ChatHubTests
    {
        [Fact]
        public async Task SendMessage_ShouldBroadcastMessage()
        {
            // Arrange
            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            // Configure the mock to return the client proxy
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.User.Identity.Name).Returns("TestUser");

            var mockMessageQueue = new Mock<IMessageQueue>();

            var chatHub = new ChatHub(mockMessageQueue.Object)
            {
                Clients = mockClients.Object,
                Context = mockContext.Object
            };

            string message = "Hello, world!";

            // Act
            await chatHub.SendMessage(message);

            // Assert
            mockClientProxy.Verify(
                proxy => proxy.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(o =>
                        (string)o[0] == "TestUser" &&
                        (string)o[1] == message),
                    default),
                Times.Once);
        }
    }
}
