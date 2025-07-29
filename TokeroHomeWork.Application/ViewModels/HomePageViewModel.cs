using System.Collections.ObjectModel;
using TokeroHomeWork.Application.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using TokeroHomeWork.Application.Interfaces;

namespace TokeroHomeWork.Application.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    private readonly ICryptoPricingRepository _cryptoPricingRepository;
    public ObservableCollection<CryptoItemViewModel> CryptoItems { get; } = new();

    public HomePageViewModel(ICryptoPricingRepository cryptoPricingRepository)
    {
        _cryptoPricingRepository = cryptoPricingRepository;
        Initialize().GetAwaiter();
        Title = "Watchlist";
    }

    private async Task Initialize()
    {
        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Bitcoin",
            Value = await GetCurrentPriceAsync("bitcoin", "eur")
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Ethereum",
            Value = await GetCurrentPriceAsync("ethereum", "eur")
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Cardano",
            Value = await GetCurrentPriceAsync("cardano", "eur")
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Solana",
            Value = await GetCurrentPriceAsync("solana", "eur")
        });
    }

    private async Task<decimal?> GetCurrentPriceAsync(string cryptoName, string currency)
    {
        return await _cryptoPricingRepository.GetCurrentPriceAsync(cryptoName, currency);
    }

    [ObservableProperty] 
    private string _title;
}