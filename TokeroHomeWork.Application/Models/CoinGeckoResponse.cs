using System.Text.Json.Serialization;

namespace TokeroHomeWork.Application.Models
{
    public class CoinGeckoHistoricalResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("market_data")]
        public MarketData? MarketData { get; set; }
    }

    public class MarketData
    {
        [JsonPropertyName("current_price")]
        public CurrentPrice? CurrentPrice { get; set; }
    }

    public class CurrentPrice
    {
        [JsonPropertyName("eur")]
        public decimal Eur { get; set; }
    }
}
