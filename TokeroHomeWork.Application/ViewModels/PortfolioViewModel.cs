using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;
using TokeroHomeWork.Application.Interfaces;
using TokeroHomeWork.Application.Models;

namespace TokeroHomeWork.Application.ViewModels;

public partial class PortfolioViewModel : ObservableObject, IQueryAttributable
{
    private ICryptoPricingRepository _cryptoPricingRepository;
    public ObservableCollection<InvestmentRecord> InvestmentRecords { get; } = new();
    public ObservableCollection<InvestmentRecord> ProcessedInvestmentRecords { get; } = new();
    public ObservableCollection<CryptoSummary> CryptoSummaries { get; } = new();

    public PortfolioViewModel(ICryptoPricingRepository cryptoPricingRepository)
    {
        _cryptoPricingRepository = cryptoPricingRepository;
    }
    
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query != null && query.Count > 0)
        {
            try
            {
                IsLoading = true;
                HasInvestmentRecords = false;
                PerformanceChart = null;
                DonutChart = null;
                
                if (query.TryGetValue("cryptoName", out var cryptoNameObj) &&
                    query.TryGetValue("startDate", out var startDateObj) &&
                    query.TryGetValue("dayOfTheMonth", out var dayOfMonthObj) &&
                    query.TryGetValue("amountPerMonth", out var amountObj) &&
                    query.TryGetValue("cryptoValue", out var cryptoValueObj))
                {
                    string cryptoName = cryptoNameObj.ToString();
                    DateTime startDate = Convert.ToDateTime(startDateObj);
                    int dayOfMonth = Convert.ToInt32(dayOfMonthObj);
                    decimal amount = Convert.ToDecimal(amountObj);
                    decimal currentValue = Convert.ToDecimal(cryptoValueObj);

                    var records = await SetInvestmentRecords(cryptoName, startDate, dayOfMonth, amount, currentValue);
                    foreach (var record in records)
                    {
                        InvestmentRecords.Add(record);
                    }
                }

                if (query.TryGetValue("selectedCoins", out var selectedCoinsObj) &&
                    query.TryGetValue("startDate", out var startDateBulkObj) &&
                    query.TryGetValue("dayOfTheMonth", out var dayOfMonthBulkObj) &&
                    query.TryGetValue("amountPerMonth", out var amountBulkObj))
                {
                    List<string> selectedCoins = (List<string>)selectedCoinsObj;
                    DateTime startDate = Convert.ToDateTime(startDateBulkObj);
                    int dayOfMonth = Convert.ToInt32(dayOfMonthBulkObj);
                    decimal amountBulk = Convert.ToDecimal(amountBulkObj);
                    decimal amountPerCoin = amountBulk / selectedCoins.Count;

                    foreach (var coin in selectedCoins)
                    {
                        var currentValue = await GetCurrentPriceAsync(coin.ToLower(), "eur");
                        var records =
                            await SetInvestmentRecords(coin, startDate, dayOfMonth, amountPerCoin, currentValue);
                        foreach (var record in records)
                        {
                            InvestmentRecords.Add(record);
                        }
                    }
                }

                HasInvestmentRecords = InvestmentRecords.Count > 0;

                if (HasInvestmentRecords)
                {
                    CalculatePortfolioProgress();
                    await CalculateCryptoSummary();
                    GeneratePerformanceChart();
                    GenerateCryptoHoldingsChart();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task ClearPortfolioAsync()
    {
        InvestmentRecords.Clear();
        ProcessedInvestmentRecords.Clear();
        HasInvestmentRecords = false;
        PerformanceChart = null;
        DonutChart = null;
    }
    

    private async Task<ObservableCollection<InvestmentRecord>> SetInvestmentRecords(string cryptoName, DateTime startDate, int dayOfMonth, decimal amount, decimal? currentValue)
    {
        DateTime currentStartDate = new DateTime(startDate.Year, startDate.Month, dayOfMonth);
        DateTime today = DateTime.Today;
        int recordCount = 0;

        ObservableCollection<InvestmentRecord> records = new();

        while (currentStartDate <= today)
        {
            var historicalPrice = await GetHistoricalPriceAsync(cryptoName, currentStartDate);
            var cryptoAmount = CalculateCryptoAmount(amount, historicalPrice);
            var roi = CalculateROI(amount, cryptoAmount, currentValue.Value);
            
            var investmentRecord = new InvestmentRecord
            {
                Date = currentStartDate,
                CryptoName = cryptoName,
                InvestedAmount = amount,
                CryptoAmount = cryptoAmount,
                CryptoValue = historicalPrice,
                ValueToday = currentValue.Value,
                ROI = roi
            };

            records.Add(investmentRecord);
                    
            recordCount++;
            currentStartDate = currentStartDate.AddMonths(1);
        }
        
        return records;
    }

    private void CalculatePortfolioProgress()
    {
        ProcessedInvestmentRecords.Clear();
        var groupedRecords = InvestmentRecords.GroupBy(r => r.CryptoName).ToList();

        foreach (var groupedRecord in groupedRecords)
        {
            var acumulatedCrypto = 0m;
            var acumulatedInvestment = 0m;
            foreach (var record in groupedRecord)
            {
                acumulatedCrypto += record.CryptoAmount;
                acumulatedInvestment += record.InvestedAmount;
                var investmentRecord = new InvestmentRecord
                {
                    Date = record.Date,
                    CryptoName = record.CryptoName,
                    InvestedAmount = acumulatedInvestment,
                    CryptoAmount = acumulatedCrypto,
                    CryptoValue = record.CryptoValue,
                    ValueToday = record.ValueToday,
                    ROI = CalculateROI(acumulatedInvestment, acumulatedCrypto, record.CryptoValue)
                };
                
                ProcessedInvestmentRecords.Add(investmentRecord);
            }
        }
    }

    private async Task CalculateCryptoSummary()
    {
        CryptoSummaries.Clear();
    
        var cryptoGroups = ProcessedInvestmentRecords
            .GroupBy(r => r.CryptoName)
            .Select(group => group.OrderBy(r => r.Date).Last())
            .ToList();

        foreach (var crypto in cryptoGroups)
        {
            var cryptoName = crypto.CryptoName.ToLower();
            var priceInBtc = await GetCurrentPriceAsync(cryptoName, "btc");
            var amountInBtc = priceInBtc * crypto.CryptoAmount;
            
            var summary = new CryptoSummary
            {
                CryptoName = crypto.CryptoName,
                Amount = crypto.CryptoAmount,
                TotalValue = crypto.CryptoAmount * crypto.ValueToday,
                AmountInBTC = amountInBtc
            };
            
            CryptoSummaries.Add(summary);
        }

    }

    private decimal CalculateCryptoAmount(decimal investedAmount, decimal? value)
    {
        if (value == null)
        {
            return 0;
        }
        
        return investedAmount / value.Value;   
    }
    
    private decimal CalculateROI(decimal amountInvested, decimal numberOfCrypto, decimal cryptoValue)
    {
        var currentValue = numberOfCrypto * cryptoValue;
        return (currentValue - amountInvested) / amountInvested * 100;
    }

    private void GeneratePerformanceChart()
    {
        var entries = new List<ChartEntry>();
        var lineColor = SKColor.Parse("#3498db");
        var firstEmptyChart = new ChartEntry(0)
        {
            Label = "0",
            Color = lineColor,
            TextColor = lineColor
        };
        var emptyEntry = new ChartEntry(0)
        {
            Color = lineColor,
            TextColor = lineColor
        };
        entries.Add(firstEmptyChart);
        entries.Add(emptyEntry);

        var groupedRecords = ProcessedInvestmentRecords
            .GroupBy(r => r.Date.Month)
            .OrderBy(g => g.Key)
            .ToList();
        var totalPortfolioValue = 0m;
        var totalPotfolioInvestment = 0m;
        var groupTotalValue = 0m;
        
        foreach (var group in groupedRecords)
        {
            var cryptoGroups = group.GroupBy(r => r.CryptoName);
            
            foreach (var cryptoGroup in cryptoGroups)
            {
                decimal cryptoValue = cryptoGroup.Sum(r => r.CryptoAmount * r.CryptoValue);
        
                groupTotalValue += cryptoValue;
            }
            
            var entry = new ChartEntry((float)groupTotalValue)
            {
                Label = "",
                ValueLabel = $"{groupTotalValue:N2} €",
                Color = lineColor,
                TextColor = lineColor
            };
            groupTotalValue = 0;
            entries.Add(entry);
        }
        
        var lastRecordPerCoin = ProcessedInvestmentRecords
            .GroupBy(r => r.CryptoName)
            .Select(group => group.OrderBy(r => r.Date).Last())
            .ToList();
        foreach (var coin in lastRecordPerCoin)
        {
            totalPortfolioValue += coin.CryptoAmount * coin.ValueToday;
            totalPotfolioInvestment += coin.InvestedAmount;
        }
        TotalROI = (totalPortfolioValue - totalPotfolioInvestment) / totalPotfolioInvestment * 100;
        PortfolioChartTitle = $"Total value: {totalPortfolioValue:N2} €  Total investment: {totalPotfolioInvestment:N2} €";
        PortfolioChartSubtitle = $"ROI: {TotalROI:F2}%";

        var newChart = new LineChart
        {
            Entries = entries,
            LineMode = LineMode.Spline,
            LineSize = 4,
            PointMode = PointMode.Circle,
            PointSize = 10,
            LabelTextSize = 30,
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            BackgroundColor = SKColors.Empty,
            MinValue = 0,
            MaxValue = (float)totalPortfolioValue * 0.5f,
            IsAnimated = false,
            ShowYAxisLines = false,
            ShowYAxisText = false,
        };
        PerformanceChart = newChart;
    }
    
    private void GenerateCryptoHoldingsChart()
    {
        var entries = new List<ChartEntry>();
        
        var coinColors = new Dictionary<string, SKColor>
        {
            { "bitcoin", SKColor.Parse("#F7931A") },   // Orange
            { "ethereum", SKColor.Parse("#627EEA") },  // Blue
            { "solana", SKColor.Parse("#00FFA3") },    // Green
            { "cardano", SKColor.Parse("#0033AD") },   // Navy Blue
            { "tether", SKColor.Parse("#26A17B") },    // Teal
            { "dogecoin", SKColor.Parse("#C2A633") },  // Gold
            { "tron", SKColor.Parse("#EF0027") }       // Red
        };

        var lastRecordPerCoin = ProcessedInvestmentRecords
            .GroupBy(r => r.CryptoName)
            .Select(group => group.OrderBy(r => r.Date).Last())
            .ToList();

        foreach (var coin in lastRecordPerCoin)
        {
            var coinValueToday = coin.CryptoAmount * coin.ValueToday;
            var color = coinColors[coin.CryptoName.ToLower()];
            var entry = new ChartEntry((float)coinValueToday)
            {
                Label = coin.CryptoName,
                ValueLabel = $"{coinValueToday:N2} €",
                Color = color,
                TextColor = color
            };
            entries.Add(entry);
        }
        
        var donutChart = new DonutChart
        {
            Entries = entries,
            LabelTextSize = 30,
            LabelMode = LabelMode.RightOnly,
            BackgroundColor = SKColors.Empty,
            HoleRadius = 0.5f,
            IsAnimated = false
        };
    
        DonutChart = donutChart;
    }
    
    private async Task<decimal> GetHistoricalPriceAsync(string cryptoName, DateTime date)
    {
        return await _cryptoPricingRepository.GetHistoricalPriceAsync(cryptoName.ToLower(),date);
    }
    
    private async Task<decimal?> GetCurrentPriceAsync(string cryptoName, string currency)
    {
        return await _cryptoPricingRepository.GetCurrentPriceAsync(cryptoName, currency);
    }


    [ObservableProperty]
    private LineChart _performanceChart;
    
    [ObservableProperty]
    private DonutChart _donutChart;

    [ObservableProperty]
    private bool _hasInvestmentRecords;
    
    [ObservableProperty]
    private string _portfolioChartTitle;
    
    [ObservableProperty]
    private string _portfolioChartSubtitle;

    [ObservableProperty] 
    private decimal _totalROI;

    [ObservableProperty] 
    private bool _isLoading;
}
