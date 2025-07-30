using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokeroHomeWork.Application.ViewModels;

namespace TokeroHomeWork.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }
}