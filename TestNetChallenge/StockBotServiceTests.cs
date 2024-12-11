using Moq;
using System.Net;
using NetChallenge.Services;
using Moq.Protected;

namespace TestNetChallenge
{
    public class StockBotServiceTests
    {
        [Fact]
        public void ParseStockCsvResponse_ShouldReturnCorrectQuote()
        {
            // Arrange
            var stockCode = "AAPL.US";
            var csvResponse = "Symbol,Date,Time,Open,High,Low,Close,Volume\n" +
                              "AAPL.US,2024-12-09,22:00:14,241.83,247.24,241.75,246.75,44549049";

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(csvResponse)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var stockBotService = new StockBotService(null);

            // Act
            var result = stockBotService.ParseStockCsvResponse(csvResponse, stockCode);

            // Assert
            Assert.Equal("AAPL.US quote is $246.75 per share.", result);
        }
    }
}
