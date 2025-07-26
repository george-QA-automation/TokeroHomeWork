using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TokeroHomeWork.Application.Models;

namespace TokeroHomeWork.Application.ViewModels;

public partial class PortfolioViewModel : ObservableObject
{
    public ObservableCollection<InvestmentRecord> InvestmentRecords { get; } = new();

    public PortfolioViewModel()
    {
        Initialize();
    }

    private void Initialize()
    {
        InvestmentRecords.Add(new InvestmentRecord
        {
            Date = DateTime.Now.AddDays(-30),
            InvestedAmount = 1000,
            CryptoAmount = 0.05m,
            CryptoName = "BTC",
            ValueToday = 1200,
            ROI = 20
        });

        InvestmentRecords.Add(new InvestmentRecord
        {
            Date = DateTime.Now.AddDays(-60),
            InvestedAmount = 500,
            CryptoAmount = 0.025m,
            CryptoName = "ETH",
            ValueToday = 650,
            ROI = 30
        });

        InvestmentRecords.Add(new InvestmentRecord
        {
            Date = DateTime.Now.AddDays(-90),
            InvestedAmount = 2000,
            CryptoAmount = 0.1m,
            CryptoName = "ADA",
            ValueToday = 2400,
            ROI = 20
        });
    }
}
