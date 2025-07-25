using System.Collections.ObjectModel;
using TokeroHomeWork.Application.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TokeroHomeWork.Application.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    public ObservableCollection<CryptoItem> CryptoItems { get; set; }

    public HomePageViewModel()
    {
        Initialize();
        Title = "Watchlist";
    }

    private void Initialize()
    {
        CryptoItems = new ObservableCollection<CryptoItem>
        {
            new CryptoItem
            {
                CryptoName = "Bitcoin",
                Value = 25000.00m
            },
            new CryptoItem
            {
                CryptoName = "Ethereum",
                Value = 3500.00m
            },
            new CryptoItem
            {
                CryptoName = "Solana",
                Value = 140.00m
            }
        };
    }

    [ObservableProperty] 
    private string _title;
}