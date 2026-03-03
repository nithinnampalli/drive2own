using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drive2Own.Mobile.Models;
using Drive2Own.Mobile.Services;

namespace Drive2Own.Mobile.ViewModels;

public partial class RoutesViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private TripRoute? selectedRoute;

    public ObservableCollection<TripRoute> Routes { get; } = new();

    public RoutesViewModel(ApiService api) => _api = api;

    [RelayCommand]
    private async Task LoadRoutesAsync()
    {
        IsBusy = true;
        try
        {
            var data = await _api.GetRoutesAsync();
            Routes.Clear();
            if (data != null)
                foreach (var r in data)
                    Routes.Add(r);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRouteAsync(TripRoute route)
    {
        bool confirm = await Shell.Current.DisplayAlert("Delete Route", $"Delete '{route.Name}'?", "Delete", "Cancel");
        if (!confirm) return;

        await _api.DeleteRouteAsync(route.Id);
        Routes.Remove(route);
    }

    [RelayCommand]
    private async Task ShareRouteAsync(TripRoute route)
    {
        var share = await _api.ShareRouteAsync(route.Id, isPublic: true);
        if (share != null)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Title = $"Share: {route.Name}",
                Text = $"Check out my route '{route.Name}' on Drive2Own!",
                Uri = share.RouteUrl
            });
        }
    }

    public string FormatDistance(double meters) =>
        meters >= 1000 ? $"{meters / 1000:F2} km" : $"{(int)meters} m";

    public string FormatDuration(double seconds)
    {
        var h = (int)(seconds / 3600);
        var m = (int)((seconds % 3600) / 60);
        return h > 0 ? $"{h}h {m}m" : $"{m}m";
    }
}
