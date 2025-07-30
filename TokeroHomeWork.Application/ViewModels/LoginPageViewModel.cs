using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TokeroHomeWork.Application.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    public LoginPageViewModel()
    {
        
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        Debug.WriteLine("Login");
        if (Username == "admin" && Password == "123456")
        {
            await Shell.Current.GoToAsync("//HomePage");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Invalid username or password", "OK");
        }
    }

    [ObservableProperty] 
    private string _username;
    
    [ObservableProperty] 
    private string _password;
}