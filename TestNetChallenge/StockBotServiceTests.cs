using NetChallenge.Services;

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

            var stockBotService = new StockBotService(null);

            // Act
            var result = stockBotService.ParseStockCsvResponse(csvResponse, stockCode);

            // Assert
            Assert.Equal("AAPL.US quote is $246.75 per share.", result);
        }
    }
}
