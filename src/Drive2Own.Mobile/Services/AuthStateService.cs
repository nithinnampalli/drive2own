namespace Drive2Own.Mobile.Services;

public class AuthStateService
{
    private const string TokenKey = "auth_token";
    private const string UserIdKey = "user_id";
    private const string DisplayNameKey = "display_name";

    public string? Token => Preferences.Get(TokenKey, null);
    public int UserId => Preferences.Get(UserIdKey, 0);
    public string? DisplayName => Preferences.Get(DisplayNameKey, null);
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public void SaveAuth(string token, int userId, string displayName)
    {
        Preferences.Set(TokenKey, token);
        Preferences.Set(UserIdKey, userId);
        Preferences.Set(DisplayNameKey, displayName);
    }

    public void ClearAuth()
    {
        Preferences.Remove(TokenKey);
        Preferences.Remove(UserIdKey);
        Preferences.Remove(DisplayNameKey);
    }
}
