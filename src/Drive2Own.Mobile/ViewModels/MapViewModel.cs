using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drive2Own.Mobile.Models;
using Drive2Own.Mobile.Services;

namespace Drive2Own.Mobile.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private double mapCenterLatitude = 51.505;
    [ObservableProperty] private double mapCenterLongitude = -0.09;
    [ObservableProperty] private string newPinTitle = string.Empty;
    [ObservableProperty] private string newPinDescription = string.Empty;
    [ObservableProperty] private double newPinLatitude;
    [ObservableProperty] private double newPinLongitude;

    public ObservableCollection<LocationPin> Pins { get; } = new();

    public MapViewModel(ApiService api)
    {
        _api = api;
        _ = LoadCurrentLocationAsync();
    }

    [RelayCommand]
    private async Task LoadPinsAsync()
    {
        IsBusy = true;
        try
        {
            var data = await _api.GetLocationPinsAsync();
            Pins.Clear();
            if (data != null)
                foreach (var p in data)
                    Pins.Add(p);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddPinAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPinTitle)) return;

        var pin = await _api.CreateLocationPinAsync(
            NewPinTitle, NewPinDescription, NewPinLatitude, NewPinLongitude);

        if (pin != null)
        {
            Pins.Add(pin);
            NewPinTitle = string.Empty;
            NewPinDescription = string.Empty;
        }
    }

    [RelayCommand]
    private async Task DeletePinAsync(LocationPin pin)
    {
        bool confirm = await Shell.Current.DisplayAlert("Delete Pin", $"Delete '{pin.Title}'?", "Delete", "Cancel");
        if (!confirm) return;
        await _api.DeleteLocationPinAsync(pin.Id);
        Pins.Remove(pin);
    }

    private async Task LoadCurrentLocationAsync()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location != null)
            {
                MapCenterLatitude = location.Latitude;
                MapCenterLongitude = location.Longitude;
            }
        }
        catch { /* ignore */ }
    }
}
