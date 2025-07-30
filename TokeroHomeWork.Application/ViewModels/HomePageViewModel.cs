using System.Collections.ObjectModel;
using TokeroHomeWork.Application.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TokeroHomeWork.Application.Interfaces;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace TokeroHomeWork.Application.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    private readonly ICryptoPricingRepository _cryptoPricingRepository;
    public ObservableCollection<CryptoItemViewModel> CryptoItems { get; } = new();
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

    public HomePageViewModel(ICryptoPricingRepository cryptoPricingRepository)
    {
        _cryptoPricingRepository = cryptoPricingRepository;
        Initialize().GetAwaiter();
        Title = "Watchlist";
    }

    private async Task Initialize()
    {
        try
        {
            IsLoading = true;

            var tasks = CryptoList.Select(async crypto => 
            {
                var price = await GetCurrentPriceAsync(crypto.ToLower(), "eur");
                return new CryptoItemViewModel(crypto, price);
            }).ToList();

            var results = await Task.WhenAll(tasks);
            foreach (var item in results)
            {
                CryptoItems.Add(item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }


    [RelayCommand]
    private async Task BulkInvestAsync()
    {
        var tcs = new TaskCompletionSource<(bool confirmed, DateTime date, int dayOfMonth, decimal amount, List<string> selectedCryptos)>();
            var selectedDate = DateTime.Now;
            var selectedDayOfMonth = 15;
            var investmentAmount = 100m;
            var selectedCryptos = new List<string>();

            var dayPicker = new Picker
            {
                Title = "Select day",
                Margin = new Thickness(0, 10),
                IsEnabled = false,
            };
            
            var datePicker = new DatePicker { Date = selectedDate, Format = "D", Margin = new Thickness(0, 10) };
            DateTime minDate = DateTime.Today.AddDays(-365); //Due to demo API limitation, can only fetch data from 365 ago
            datePicker.MinimumDate = minDate;
            datePicker.DateSelected += (s, e) =>
            {
                selectedDate = e.NewDate;
                dayPicker.IsEnabled = true;
                int remainingDaysInMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);
                for (int i = selectedDate.Day; i <= remainingDaysInMonth; i++)
                {
                    dayPicker.Items.Add(i.ToString());
                }
            };
            
            dayPicker.SelectedIndexChanged += (s, e) => 
            {
                if (dayPicker.SelectedIndex >= 0)
                {
                    selectedDayOfMonth = int.Parse(dayPicker.Items[dayPicker.SelectedIndex]);
                }
            };

            var amountEntry = new Entry
            {
                Placeholder = "Enter amount in EUR",
                Keyboard = Keyboard.Numeric,
                Margin = new Thickness(0, 10)
            };
            amountEntry.TextChanged += (s, e) =>
            {
                if (decimal.TryParse(amountEntry.Text, out decimal amount))
                {
                    investmentAmount = amount;
                }
            };

            var cryptoSelectionStack = new StackLayout { Spacing = 10, Margin = new Thickness(0, 10) };

            foreach (var cryptoName in CryptoList)
            {
                var crypto = CryptoItems.FirstOrDefault(c => c.CryptoName == cryptoName);
                var price = crypto?.Value ?? 0;
                var horizontalLayout = new HorizontalStackLayout { VerticalOptions = LayoutOptions.Center };
                var checkBox = new CheckBox
                {
                    Color = Colors.BlueViolet, IsChecked = false, VerticalOptions = LayoutOptions.Center
                };
                var cryptoLabel = new Label
                {
                    Text = $"{cryptoName} - {price:N2} €", VerticalOptions = LayoutOptions.Center, FontSize = 14
                };
                horizontalLayout.Children.Add(checkBox);
                horizontalLayout.Children.Add(cryptoLabel);

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
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => { checkBox.IsChecked = !checkBox.IsChecked; };
                horizontalLayout.GestureRecognizers.Add(tapGesture);

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
                        Text = $"Bulk Invest",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 0, 0, 15)
                    },
                    new Label { Text = "Select cryptocurrencies to invest in:", FontSize = 16 },
                    cryptoScrollView,
                    new Label { Text = "Select start investment date:", FontSize = 16 },
                    datePicker,
                    new Label { Text = "Select day of the month:", FontSize = 16 },
                    dayPicker,
                    new Label { Text = "Investment amount will be distributed equally between selected coins:", FontSize = 16 },
                    amountEntry,
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
                tcs.SetResult((true, selectedDate, selectedDayOfMonth, investmentAmount, selectedCryptos));
            };

            cancelButton.Clicked += async (s, e) =>
            {
                tcs.SetResult((false, DateTime.Now, 0, 0, new List<string>()));
                await Shell.Current.Navigation.PopModalAsync();
            };

            var result = await tcs.Task;
            
            if (result.confirmed)
            {
                var navigationParameters = new ShellNavigationQueryParameters
                {
                    {"selectedCoins", result.selectedCryptos},
                    {"startDate", result.date},
                    {"dayOfTheMonth", result.dayOfMonth},
                    {"amountPerMonth", result.amount},
                };
                await Shell.Current.GoToAsync("//Portfolio", navigationParameters);
            }
    }

    private async Task<decimal?> GetCurrentPriceAsync(string cryptoName, string currency)
    {
        return await _cryptoPricingRepository.GetCurrentPriceAsync(cryptoName, currency);
    }

    [ObservableProperty] 
    private string _title;

    [ObservableProperty] 
    private bool _isLoading;
}