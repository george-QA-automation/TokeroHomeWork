using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microcharts;
using SkiaSharp;
using TokeroHomeWork.Application.Interfaces;
using TokeroHomeWork.Application.Models;

namespace TokeroHomeWork.Application.ViewModels;

public partial class PortfolioViewModel : ObservableObject, IQueryAttributable
{
    private ICryptoPricingRepository _cryptoPricingRepository;
    public ObservableCollection<InvestmentRecord> InvestmentRecords { get; } = new();

    public PortfolioViewModel(ICryptoPricingRepository cryptoPricingRepository)
    {
        _cryptoPricingRepository = cryptoPricingRepository;
    }
    
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query != null && query.Count > 0)
        {
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
                
                var sortedRecords = new ObservableCollection<InvestmentRecord>(
                    InvestmentRecords.OrderBy(r => r.Date)
                );

                InvestmentRecords.Clear();
                foreach (var record in sortedRecords)
                {
                    InvestmentRecords.Add(record);
                }

                HasInvestmentRecords = InvestmentRecords.Count > 0;

                if (HasInvestmentRecords)
                {
                    GeneratePerformanceChart();
                }
            }
        }
    }

    private async Task<ObservableCollection<InvestmentRecord>> SetInvestmentRecords(string cryptoName, DateTime startDate, int dayOfMonth, decimal amount, decimal currentValue)
    {
        DateTime currentStartDate = new DateTime(startDate.Year, startDate.Month, dayOfMonth);
        DateTime today = DateTime.Today;
        int recordCount = 0;

        ObservableCollection<InvestmentRecord> records = new();

        while (currentStartDate <= today)
        {
            var historicalPrice = await GetHistoricalPriceAsync(cryptoName, currentStartDate);
            var cryptoAmount = CalculateCryptoAmount(amount, historicalPrice);
            var roi = CalculateROI(amount, cryptoAmount, currentValue);
            
            var investmentRecord = new InvestmentRecord
            {
                Date = currentStartDate,
                CryptoName = cryptoName,
                InvestedAmount = amount,
                CryptoAmount = cryptoAmount,
                CryptoValue = historicalPrice,
                ValueToday = currentValue,
                ROI = roi
            };

            records.Add(investmentRecord);
                    
            recordCount++;
            currentStartDate = currentStartDate.AddMonths(1);
        }
        
        return records;
    }
    
    private async Task<decimal?> GetHistoricalPriceAsync(string cryptoName, DateTime date)
    {
        return await _cryptoPricingRepository.GetHistoricalPriceAsync(cryptoName.ToLower(),date);
    }

    private decimal CalculateCryptoAmount(decimal investedAmount, decimal? value)
    {
        if (value == null)
        {
            return 0;
        }
        
        return investedAmount / value.Value;   
    }
    
    private decimal CalculateROI(decimal amountInvested, decimal numberOfCrypto, decimal? priceToday)
    {
        var currentValue = numberOfCrypto * priceToday!.Value;
        return (currentValue - amountInvested) / amountInvested * 100;
    }

    private void GeneratePerformanceChart()
    {
        var entries = new List<ChartEntry>();
        var lineColor = SKColor.Parse("#3498db");

        var groupedRecords = InvestmentRecords.GroupBy(r => r.Date.Month).ToList();
        decimal? totalPortfolioValue = 0m;
        
        var firstGroup = groupedRecords.First().ToList();
        var firstCryptoGroup = firstGroup.GroupBy(r => r.CryptoName).ToList();
        decimal firstTotalValue = 0;
        foreach (var cryptoGroup in firstCryptoGroup)
        {
            decimal cryptoValue = cryptoGroup.Sum(r => r.CryptoAmount * r.ValueToday);
            firstTotalValue += cryptoValue;
        }
        totalPortfolioValue += firstTotalValue;
        var firstEntry = new ChartEntry((float)firstTotalValue!)
        {
            Label = firstGroup.First().Date.ToString("MMM dd, yyyy"),
            ValueLabel = firstTotalValue.ToString("C"),
            Color = lineColor,
            TextColor = lineColor
        };
        entries.Add(firstEntry);
        
        foreach (var group in groupedRecords.Skip(1))
        {
            var records = group.ToList();
    
            decimal groupTotalValue = 0;
            var cryptoGroups = records.GroupBy(r => r.CryptoName);
    
            foreach (var cryptoGroup in cryptoGroups)
            {
                decimal cryptoValue = cryptoGroup.Sum(r => r.CryptoAmount * r.ValueToday);
        
                groupTotalValue += cryptoValue;
            }
    
            totalPortfolioValue += groupTotalValue;
            var entry = new ChartEntry((float)groupTotalValue)
            {
                Label = "",
                ValueLabel = groupTotalValue.ToString("C"),
                Color = lineColor,
                TextColor = lineColor
            };
    
            entries.Add(entry);
        }
        
        decimal minValue = 0;
        var maxValue = totalPortfolioValue * 0.5m;

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
            MinValue = (float)minValue,
            MaxValue = (float)maxValue!,
            IsAnimated = false,
            ShowYAxisLines = false,
            ShowYAxisText = false,
        };
        PerformanceChart = newChart;

        PortfolioChartTitle = $"Performance Over Time: ${totalPortfolioValue:N2}";
    }

    [ObservableProperty]
    private LineChart _performanceChart;

    [ObservableProperty]
    private bool _hasInvestmentRecords;
    
    [ObservableProperty]
    private string _portfolioChartTitle;


}
