using TokeroHomeWork.Constants;

namespace TokeroHomeWork;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (IsLoggedIn())
            {
                await GoToAsync("//HomePage");
            }
            else
            {
                MarkAsLoggedIn();
                await GoToAsync("//Login");
            }
        });
        
        bool IsLoggedIn()
        {
            return Preferences.Get(AppConstants.IsLoggedIn, "false") == "true";
        }
        
        void MarkAsLoggedIn()
        {
            Preferences.Set(AppConstants.IsLoggedIn, "true");
        }
    }
}