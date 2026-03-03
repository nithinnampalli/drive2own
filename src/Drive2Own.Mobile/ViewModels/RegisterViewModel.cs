using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drive2Own.Mobile.Services;

namespace Drive2Own.Mobile.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthStateService _authState;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string displayName = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    public RegisterViewModel(ApiService api, AuthStateService authState)
    {
        _api = api;
        _authState = authState;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(DisplayName))
        {
            ErrorMessage = "Please fill in all fields";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _api.RegisterAsync(Username, Email, Password, DisplayName);
            if (result == null)
            {
                ErrorMessage = "Registration failed. Username or email may already be taken.";
                return;
            }
            _authState.SaveAuth(result.Token, result.User.Id, result.User.DisplayName);
            _api.SetAuthToken(result.Token);
            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync() =>
        await Shell.Current.GoToAsync("//login");
}
