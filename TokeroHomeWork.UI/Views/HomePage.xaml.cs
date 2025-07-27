using TokeroHomeWork.Application.ViewModels;

namespace TokeroHomeWork.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomePageViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }
}