using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using TokeroHomeWork.Application.ViewModels;

namespace TokeroHomeWork.Application.ViewModels;
    
    public partial class CryptoItemViewModel : ObservableObject
    {
        public CryptoItemViewModel()
        {
            
        }

        [RelayCommand]
        private async Task ExecuteInvestAsync()
        {
            var tcs = new TaskCompletionSource<(bool confirmed, DateTime date, int dayOfMonth, decimal amount)>();
            var selectedDate = DateTime.Now;
            var selectedDayOfMonth = 15;
            var investmentAmount = 100m;

            var datePicker = new DatePicker { Date = selectedDate, Format = "D", Margin = new Thickness(0, 10) };
            datePicker.DateSelected += (s, e) => { selectedDate = e.NewDate; };

            var dayPicker = new Picker
            {
                Title = "Select day",
                Margin = new Thickness(0, 10)
            };
            dayPicker.Items.Add("15");
            dayPicker.Items.Add("20");
            dayPicker.Items.Add("25");
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
                        Text = $"Invest in {CryptoName}",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 0, 0, 15)
                    },
                    new Label { Text = "Select start investment date:", FontSize = 16 },
                    datePicker,
                    new Label { Text = "Select day of the month:", FontSize = 16 },
                    dayPicker,
                    new Label { Text = "Investment amount:", FontSize = 16 },
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
                tcs.SetResult((true, selectedDate, selectedDayOfMonth, investmentAmount));
            };

            cancelButton.Clicked += async (s, e) =>
            {
                tcs.SetResult((false, DateTime.Now, 0, 0));
                await Shell.Current.Navigation.PopModalAsync();
            };

            var result = await tcs.Task;
            
            if (result.confirmed)
            {
                var navigationParameters = new ShellNavigationQueryParameters
                {
                    {"cryptoName", CryptoName},
                    {"startDate", tcs.Task.Result.date},
                    {"dayOfTheMonth", tcs.Task.Result.dayOfMonth},
                    {"amountPerMonth", tcs.Task.Result.amount},
                    {"cryptoValue", Value}
                };
                await Shell.Current.GoToAsync("//Portfolio", navigationParameters);
            }
        }
        
        [ObservableProperty]
        private string _cryptoName;

        [ObservableProperty]
        private decimal? _value;
    }