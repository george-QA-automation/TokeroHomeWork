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
    public ObservableCollection<InvestmentRecord> CompareProcessedInvestmentRecords { get; } = new();
    public ObservableCollection<CryptoSummary> CryptoSummaries { get; } = new();
    public List<string> SelectedCoins { get; } = new();
    private List<string> CryptoList = new()
    { 
        "Bitcoin", 
        "Ethereum", 
        "Solana", 
        "Cardano", 
        "Tether", 
        "Dogecoin", 
        "Tron" 
    };

    private Dictionary<string, SKColor> CryptoColors = new()
    {
        { "bitcoin", SKColor.Parse("#F7931A") },   // Orange
        { "ethereum", SKColor.Parse("#800080") },  // Purple
        { "solana", SKColor.Parse("#00FFA3") },    // Green
        { "cardano", SKColor.Parse("#0033AD") },   // Navy Blue
        { "tether", SKColor.Parse("#26A17B") },    // Teal
        { "dogecoin", SKColor.Parse("#C2A633") },  // Gold
        { "tron", SKColor.Parse("#EF0027") }       // Red
    };

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
                    CryptoName = cryptoNameObj.ToString();
                    StartDate = Convert.ToDateTime(startDateObj);
                    DayOfMonth = Convert.ToInt32(dayOfMonthObj);
                    AmountPerMonth = Convert.ToDecimal(amountObj);
                    CryptoValue = Convert.ToDecimal(cryptoValueObj);

                    var records = await SetInvestmentRecords(CryptoName, StartDate, DayOfMonth, AmountPerMonth, CryptoValue);
                    foreach (var record in records)
                    {
                        InvestmentRecords.Add(record);
                    }

                    PerformanceChartLegend += $"{CryptoName} ";
                }

                if (query.TryGetValue("selectedCoins", out var selectedCoinsObj) &&
                    query.TryGetValue("startDate", out var startDateBulkObj) &&
                    query.TryGetValue("dayOfTheMonth", out var dayOfMonthBulkObj) &&
                    query.TryGetValue("amountPerMonth", out var amountBulkObj))
                {
                    List<string> selectedCoins = (List<string>)selectedCoinsObj;
                    StartDate = Convert.ToDateTime(startDateBulkObj);
                    DayOfMonth = Convert.ToInt32(dayOfMonthBulkObj);
                    AmountPerMonth = Convert.ToDecimal(amountBulkObj);
                    decimal amountPerCoin = AmountPerMonth / selectedCoins.Count;

                    foreach (var coin in selectedCoins)
                    {
                        var currentValue = await GetCurrentPriceAsync(coin.ToLower(), "eur");
                        var records = await SetInvestmentRecords(coin, StartDate, DayOfMonth, amountPerCoin, currentValue);
                        foreach (var record in records)
                        {
                            InvestmentRecords.Add(record);
                        }
                        
                        PerformanceChartLegend += $"{coin} ";
                    }
                }
                HasInvestmentRecords = InvestmentRecords.Count > 0;
                if (HasInvestmentRecords)
                {
                    CalculatePortfolioProgress(InvestmentRecords);
                    await CalculateCryptoSummary();
                    PerformanceChart = GeneratePerformanceChart(ProcessedInvestmentRecords,  SKColor.Parse("#3498db"));
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
        IsCompare = false;
        PerformanceChart = null;
        PerformanceCompareChart = null;
        DonutChart = null;
    }

    [RelayCommand]
    private async Task CompareWithAsync()
    {
        var tcs = new TaskCompletionSource<(bool confirmed, List<string> selectedCryptos)>();
        var selectedCryptoName = "";
        var selectedCryptos = new List<string>();
        var cryptoSelectionStack = new StackLayout { Spacing = 10, Margin = new Thickness(0, 10) };
        foreach (var cryptoName in CryptoList)
        {
            var horizontalLayout = new HorizontalStackLayout { VerticalOptions = LayoutOptions.Center };
            var checkBox = new CheckBox
            {
                Color = Colors.BlueViolet, IsChecked = false, VerticalOptions = LayoutOptions.Center
            };            
            var cryptoLabel = new Label { Text = $"{cryptoName} €", VerticalOptions = LayoutOptions.Center, FontSize = 14 };
            
            checkBox.CheckedChanged += (s, e) =>
            {
                if (checkBox.IsChecked)
                {
                    if (!selectedCryptos.Contains(cryptoName))
                    {
                        selectedCryptos.Add(cryptoName);
                    }
                }
                else
                {
                    selectedCryptos.Remove(cryptoName);
                }
            };

            horizontalLayout.Children.Add(checkBox);
            horizontalLayout.Children.Add(cryptoLabel);
            
            cryptoSelectionStack.Children.Add(horizontalLayout);
        }
        
        var cryptoScrollView = new ScrollView { Content = cryptoSelectionStack, MaximumHeightRequest = 200 };
        
        var selectButton = new Button
        {
            Text = "Select",
            BackgroundColor = Colors.BlueViolet,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 10, 0, 5)
        };
        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.LightGray,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 5, 0, 0)
        };
        
        var dialogContent = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 10,
            Children =
            {
                new Label
                {
                    Text = "Choose coin to compare with:",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 0, 0, 15)
                },
                cryptoScrollView,
                selectButton,
                cancelButton
            }
        };
        
        var dialogPage = new ContentPage
        {
            Content = new Frame
            {
                Content = dialogContent,
                BorderColor = Colors.LightGray,
                CornerRadius = 10,
                HasShadow = true,
                Padding = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 300
            }
        };
            
        dialogPage.SetValue(NavigationPage.HasNavigationBarProperty, false);
        await Shell.Current.Navigation.PushModalAsync(dialogPage);
        
        selectButton.Clicked += async (s, e) =>
        {
            tcs.SetResult((true, selectedCryptos));
        };

        cancelButton.Clicked += async (s, e) =>
        {
            tcs.SetResult((false, new List<string>()));
            await Shell.Current.Navigation.PopModalAsync();
        };

        var result = await tcs.Task;
        
        if (result.confirmed)
        {
            IsCompare = true;
            await Shell.Current.Navigation.PopModalAsync();
            foreach (var coin in selectedCryptos)
            {
                var valueToday = await GetCurrentPriceAsync(coin.ToLower(), "eur");
                var records = await SetInvestmentRecords(coin, StartDate, DayOfMonth, AmountPerMonth, valueToday);
                CalculatePortfolioProgress(records);
                var color = CryptoColors[coin.ToLower()];
                PerformanceCompareChart = GeneratePerformanceChart(CompareProcessedInvestmentRecords, color);
                SelectedCoins.Add(coin);
            }
        }
    }

    private async Task<ObservableCollection<InvestmentRecord>> SetInvestmentRecords(string cryptoName, DateTime startDate, int dayOfMonth, decimal amount, decimal? currentValue)
    {
        DateTime currentStartDate = new DateTime(startDate.Year, startDate.Month, dayOfMonth);
        DateTime today = DateTime.Today;

        ObservableCollection<InvestmentRecord> records = new();

        while (currentStartDate <= today)
        {
            //Set day of month according to number of days in each month
            //examle: if user selects the 30th for investment, set 28 for february
            var daysInMonth = DateTime.DaysInMonth(currentStartDate.Year, currentStartDate.Month);
            currentStartDate = currentStartDate.Day >= daysInMonth 
                ? new DateTime(currentStartDate.Year, currentStartDate.Month, daysInMonth) 
                : new DateTime(currentStartDate.Year, currentStartDate.Month, dayOfMonth);
            
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
            currentStartDate = currentStartDate.AddMonths(1);
        }
        
        return records;
    }

    private void CalculatePortfolioProgress(ObservableCollection<InvestmentRecord> investmentRecords)
    {
        ProcessedInvestmentRecords.Clear();
        var groupedRecords = investmentRecords.GroupBy(r => r.CryptoName).ToList();
        ObservableCollection<InvestmentRecord> newRecords = new();

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
                if (!IsCompare)
                {
                    ProcessedInvestmentRecords.Add(investmentRecord);
                }
                else
                {
                    CompareProcessedInvestmentRecords.Add(investmentRecord);   
                }
                
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
        return investedAmount / value!.Value;   
    }
    
    private decimal CalculateROI(decimal amountInvested, decimal numberOfCrypto, decimal cryptoValue)
    {
        var currentValue = numberOfCrypto * cryptoValue;
        return (currentValue - amountInvested) / amountInvested * 100;
    }

    private LineChart GeneratePerformanceChart(ObservableCollection<InvestmentRecord> investmentRecords, SKColor lineColorHex)
    {
        var entries = new List<ChartEntry>();
        var firstEmptyChart = new ChartEntry(0)
        {
            Label = investmentRecords.First().Date.AddMonths(-2).ToString("MMM"),
            Color = lineColorHex,
            TextColor = lineColorHex
        };
        var emptyEntry = new ChartEntry(0)
        {
            Color = lineColorHex,
            TextColor = lineColorHex
        };
        entries.Add(firstEmptyChart);
        entries.Add(emptyEntry);

        var groupedRecords = investmentRecords
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
                Color = lineColorHex,
                TextColor = lineColorHex
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

        //Update only for original chart
        if (!IsCompare)
        {
            TotalROI = (totalPortfolioValue - totalPotfolioInvestment) / totalPotfolioInvestment * 100;
            PortfolioChartInvestment = $"Total investment: {totalPotfolioInvestment:N2} €";
            PortfolioChartValue = $"Total value: {totalPortfolioValue:N2} €";
            PortfolioChartROI = $"ROI: {TotalROI:F2}%";
        }

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
            LineAreaAlpha = 0,
            MinValue = 0,
            MaxValue = (float)totalPortfolioValue * 0.5f,
            IsAnimated = false,
            ShowYAxisLines = false,
            ShowYAxisText = false,
        };
        
        return newChart;
    }
    
    private void GenerateCryptoHoldingsChart()
    {
        var entries = new List<ChartEntry>();
        
        var lastRecordPerCoin = ProcessedInvestmentRecords
            .GroupBy(r => r.CryptoName)
            .Select(group => group.OrderBy(r => r.Date).Last())
            .ToList();

        foreach (var coin in lastRecordPerCoin)
        {
            var coinValueToday = coin.CryptoAmount * coin.ValueToday;
            var color = CryptoColors[coin.CryptoName.ToLower()];
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
    private LineChart _performanceCompareChart;
    
    [ObservableProperty]
    private DonutChart _donutChart;

    [ObservableProperty]
    private bool _hasInvestmentRecords;
    
    [ObservableProperty]
    private string _portfolioChartInvestment;
    
    [ObservableProperty]
    private string _portfolioChartValue;
    
    [ObservableProperty]
    private string _portfolioChartROI;

    [ObservableProperty] 
    private decimal _totalROI;

    [ObservableProperty] 
    private bool _isLoading;
    
    [ObservableProperty] 
    private string _cryptoName;
    
    [ObservableProperty]
    private DateTime _startDate;
    
    [ObservableProperty]
    private int _dayOfMonth;
    
    [ObservableProperty]
    private decimal _amountPerMonth;
    
    [ObservableProperty]
    private decimal _cryptoValue;

    [ObservableProperty] 
    private bool _isCompare;

    [ObservableProperty] 
    private string _performanceChartLegend;
}
