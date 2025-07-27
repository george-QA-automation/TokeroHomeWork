using TokeroHomeWork.Application.ViewModels;

namespace TokeroHomeWork.Views;

public partial class PortfolioPage
{
    public PortfolioPage(PortfolioViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }
}
