namespace TokeroHomeWork;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await GoToAsync("//Login");
        });
    }
}