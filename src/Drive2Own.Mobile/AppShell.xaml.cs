using Drive2Own.Mobile.Services;

namespace Drive2Own.Mobile;

public partial class AppShell : Shell
{
    private readonly AuthStateService _authState;

    public AppShell(AuthStateService authState)
    {
        InitializeComponent();
        _authState = authState;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_authState.IsAuthenticated)
        {
            await GoToAsync("//login");
        }
        else
        {
            await GoToAsync("//main");
        }
    }
}
