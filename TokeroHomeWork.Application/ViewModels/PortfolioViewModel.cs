using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TokeroHomeWork.Application.Models;

namespace TokeroHomeWork.Application.ViewModels;

public partial class PortfolioViewModel : ObservableObject, IQueryAttributable
{
    public ObservableCollection<InvestmentRecord> InvestmentRecords { get; } = new();

    public PortfolioViewModel()
    {
        
    }
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query != null && query.Count > 0)
        {
            if (query.TryGetValue("cryptoName", out var cryptoNameObj) &&
                query.TryGetValue("startDate", out var startDateObj) &&
                query.TryGetValue("dayOfTheMonth", out var dayOfMonthObj) &&
                query.TryGetValue("amountPerMonth", out var amountObj))
            {
                string cryptoName = cryptoNameObj?.ToString();
                DateTime startDate = startDateObj is DateTime date ? date : DateTime.Now;
                int dayOfMonth = dayOfMonthObj is int day ? day : 
                    (int.TryParse(dayOfMonthObj?.ToString(), out int parsedDay) ? parsedDay : 1);
                decimal amount = amountObj is decimal amt ? amt :
                    (decimal.TryParse(amountObj?.ToString(), out decimal parsedAmt) ? parsedAmt : 0);
                
                var records = SetInvestmentRecords(cryptoName, startDate, dayOfMonth, amount);
                foreach (var record in records)
                {
                    InvestmentRecords.Add(record);
                }
            }
        }
    }

    private ObservableCollection<InvestmentRecord> SetInvestmentRecords(string cryptoName, DateTime startDate, int dayOfMonth, decimal amount)
    {
        DateTime currentDate = new DateTime(startDate.Year, startDate.Month, dayOfMonth);
        DateTime today = DateTime.Today;
        int recordCount = 0;

        ObservableCollection<InvestmentRecord> records = new();

        while (currentDate <= today)
        {
            var investmentRecord = new InvestmentRecord
            {
                Date = currentDate,
                CryptoName = cryptoName,
                InvestedAmount = amount,
                CryptoAmount = 0.05m,
                ValueToday = 1000,
                ROI = 20
            };

            records.Add(investmentRecord);
                    
            recordCount++;
            currentDate = currentDate.AddMonths(1);
        }
        
        return records;
    }
}
