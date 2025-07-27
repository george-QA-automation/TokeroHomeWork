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
            Value = await GetCurrentPriceAsync("bitcoin")
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Ethereum",
            Value = await GetCurrentPriceAsync("ethereum")
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Cardano",
            Value = await GetCurrentPriceAsync("cardano")
        });

        CryptoItems.Add(new CryptoItemViewModel
        {
            CryptoName = "Solana",
            Value = await GetCurrentPriceAsync("solana")
        });
    }

    private async Task<decimal?> GetCurrentPriceAsync(string cryptoName)
    {
        return await _cryptoPricingRepository.GetCurrentPriceAsync(cryptoName);
    }

    [ObservableProperty] 
    private string _title;
}