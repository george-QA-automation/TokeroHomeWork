using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
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
                string cryptoName = cryptoNameObj?.ToString();
                DateTime startDate = startDateObj is DateTime date ? date : DateTime.Now;
                int dayOfMonth = dayOfMonthObj is int day ? day : 
                    (int.TryParse(dayOfMonthObj?.ToString(), out int parsedDay) ? parsedDay : 1);
                decimal amount = amountObj is decimal amt ? amt :
                    (decimal.TryParse(amountObj?.ToString(), out decimal parsedAmt) ? parsedAmt : 0);
                decimal currentValue = cryptoValueObj is decimal val ? val :
                    (decimal.TryParse(amountObj?.ToString(), out decimal parsedValue) ? parsedValue : 0);
                
                var records = await SetInvestmentRecords(cryptoName, startDate, dayOfMonth, amount, currentValue);
                foreach (var record in records)
                {
                    InvestmentRecords.Add(record);
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
    
    private async Task<decimal?> GetCurrentPriceAsync(string cryptoName)
    {
        return await _cryptoPricingRepository.GetCurrentPriceAsync(cryptoName.ToLower());
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
        var currentValue = numberOfCrypto * priceToday.Value;
        return (currentValue - amountInvested) / amountInvested * 100;
    }

}
