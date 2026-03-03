using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drive2Own.Mobile.Models;
using Drive2Own.Mobile.Services;

namespace Drive2Own.Mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthStateService _authState;

    [ObservableProperty] private DashboardData? dashboardData;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string totalDistance = "0 km";
    [ObservableProperty] private string totalDuration = "0m";
    [ObservableProperty] private string welcomeMessage = string.Empty;

    public ObservableCollection<RouteMetric> RecentRoutes { get; } = new();

    public DashboardViewModel(ApiService api, AuthStateService authState)
    {
        _api = api;
        _authState = authState;
        WelcomeMessage = $"Welcome, {authState.DisplayName}!";
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsBusy = true;
        try
        {
            DashboardData = await _api.GetDashboardAsync();
            if (DashboardData != null)
            {
                TotalDistance = FormatDistance(DashboardData.TotalDistanceMeters);
                TotalDuration = FormatDuration(DashboardData.TotalDurationSeconds);
                RecentRoutes.Clear();
                foreach (var r in DashboardData.RecentRoutes)
                    RecentRoutes.Add(r);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string FormatDistance(double meters) =>
        meters >= 1000 ? $"{meters / 1000:F2} km" : $"{(int)meters} m";

    private static string FormatDuration(double seconds)
    {
        var h = (int)(seconds / 3600);
        var m = (int)((seconds % 3600) / 60);
        return h > 0 ? $"{h}h {m}m" : $"{m}m";
    }
}
