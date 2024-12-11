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
            var mockGroupProxy = new Mock<IClientProxy>();

            // Configure the mock to return the group proxy
            mockClients.Setup(clients => clients.Group("1")).Returns(mockGroupProxy.Object);

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
            await chatHub.SendMessage("1", message);

            // Assert
            mockGroupProxy.Verify(
                proxy => proxy.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(o =>
                        (string)o[1] == "TestUser" &&
                        (string)o[2] == message),
                    default),
                Times.Once);
        }
    }
}
