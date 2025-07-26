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
            var tcs = new TaskCompletionSource<(bool confirmed, DateTime date)>();
            var selectedDate = DateTime.Now;
            var datePicker = new DatePicker { Date = selectedDate, Format = "D", Margin = new Thickness(0, 10) };
            datePicker.DateSelected += (s, e) => { selectedDate = e.NewDate; };

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
                    new Label { Text = "Select investment date:", FontSize = 16 },
                    datePicker,
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

            selectButton.Clicked += async (s, e) =>
            {
                tcs.SetResult((true, selectedDate));
                await Shell.Current.Navigation.PopModalAsync();
            };

            cancelButton.Clicked += async (s, e) =>
            {
                tcs.SetResult((false, DateTime.Now));
                await Shell.Current.Navigation.PopModalAsync();
            };

            dialogPage.SetValue(NavigationPage.HasNavigationBarProperty, false);
            await Shell.Current.Navigation.PushModalAsync(dialogPage);

            var result = await tcs.Task;

            // Handle the result
            // if (result.confirmed)
            // {
            //     await NavigateToInvestmentDetailsPage(result.date);
            // }
        }
        
        [ObservableProperty]
        private string _cryptoName;

        [ObservableProperty]
        private decimal _value;
    }