using System.Collections.ObjectModel;
using TokeroHomeWork.Application.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TokeroHomeWork.Application.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    public ObservableCollection<CryptoItemViewModel> CryptoItems { get; } = new();

    public HomePageViewModel()
    {
        Initialize();
        Title = "Watchlist";
    }

    private void Initialize()
    {
        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Bitcoin",
            Value = 50000.00m
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Ethereum",
            Value = 3000.00m
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Cardano",
            Value = 2.50m
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Solana",
            Value = 120.00m
        });
    }

    [ObservableProperty] 
    private string _title;
}