using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;
using TokeroHomeWork.Application.Interfaces;
using TokeroHomeWork.Application.Models;

namespace TokeroHomeWork.Application.Repositories
{
    public class CryptoPricingRepository : ICryptoPricingRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "CG-Skgquc9Cv4f9D5SdAvv23t5q";

        public CryptoPricingRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.Timeout = TimeSpan.FromSeconds(15);

        }

        public async Task<decimal> GetHistoricalPriceAsync(string coinId, DateTime date)
        {
            try
            {
                string formattedDate = date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://api.coingecko.com/api/v3/coins/{coinId}/history?date={formattedDate}&localization=false"),
                    Headers =
                    {
                        { "accept", "application/json" },
                        { "x-cg-demo-api-key", "CG-Skgquc9Cv4f9D5SdAvv23t5q" },
                    },
                };
                
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var historicalData = JsonSerializer.Deserialize<CoinGeckoHistoricalResponse>(content);

                    if (historicalData?.MarketData?.CurrentPrice != null)
                    {
                        return historicalData.MarketData.CurrentPrice.Eur;
                    }
                }
                else
                {
                    Console.WriteLine($"Error fetching historical price: {response.StatusCode}");
                }

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetHistoricalPriceAsync: {ex.Message}");
                return 1;
            }
        }

        public async Task<decimal?> GetCurrentPriceAsync(string coinId)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://api.coingecko.com/api/v3/simple/price?vs_currencies=eur&ids={coinId}"),
                    Headers =
                    {
                        { "accept", "application/json" },
                        { "x-cg-demo-api-key", "CG-Skgquc9Cv4f9D5SdAvv23t5q" },
                    },
                };
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                
                    using JsonDocument doc = JsonDocument.Parse(content);
                
                    if (doc.RootElement.TryGetProperty(coinId, out JsonElement coinElement) &&
                        coinElement.TryGetProperty("eur", out JsonElement eurElement))
                    {
                        if (eurElement.ValueKind == JsonValueKind.Number)
                        {
                            return eurElement.GetDecimal();
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error fetching current price: {response.StatusCode}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetCurrentPriceAsync: {ex.Message}");
                return null;
            }
        }
    }
}
