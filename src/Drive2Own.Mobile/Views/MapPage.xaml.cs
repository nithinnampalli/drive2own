using Drive2Own.Mobile.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Drive2Own.Mobile.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _vm;
    private bool _pinMode;

    public MapPage(MapViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        MainMap.MapClicked += OnMapClicked;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadPinsAsync();
        _vm.LoadPinsCommand.Execute(null);
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        _vm.NewPinLatitude = e.Location.Latitude;
        _vm.NewPinLongitude = e.Location.Longitude;
    }

    private async Task LoadPinsAsync()
    {
        await Task.Delay(500);
        foreach (var pin in _vm.Pins)
        {
            var mapPin = new Pin
            {
                Label = pin.Title,
                Address = pin.Description ?? string.Empty,
                Location = new Location(pin.Latitude, pin.Longitude),
                Type = PinType.Place
            };
            MainMap.Pins.Add(mapPin);
        }
    }
}
