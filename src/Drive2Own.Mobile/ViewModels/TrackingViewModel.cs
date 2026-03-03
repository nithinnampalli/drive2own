using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drive2Own.Mobile.Models;
using Drive2Own.Mobile.Services;

namespace Drive2Own.Mobile.ViewModels;

public partial class TrackingViewModel : ObservableObject
{
    private readonly ApiService _api;
    private IGeolocation _geolocation;
    private CancellationTokenSource? _trackingCts;
    private TripRoute? _activeRoute;
    private readonly List<RoutePoint> _points = new();
    private int _sequence;

    [ObservableProperty] private bool isTracking;
    [ObservableProperty] private string trackingStatus = "Press Start to begin tracking";
    [ObservableProperty] private string routeName = string.Empty;
    [ObservableProperty] private string selectedMode = "Car";
    [ObservableProperty] private string distance = "0 m";
    [ObservableProperty] private string duration = "0m";
    [ObservableProperty] private int pointCount;
    [ObservableProperty] private double currentLatitude;
    [ObservableProperty] private double currentLongitude;

    public ObservableCollection<string> TravelModes { get; } = new() { "Car", "Walk", "Cycle", "Run" };

    public TrackingViewModel(ApiService api, IGeolocation geolocation)
    {
        _api = api;
        _geolocation = geolocation;
    }

    [RelayCommand]
    private async Task StartTrackingAsync()
    {
        if (string.IsNullOrWhiteSpace(RouteName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a route name", "OK");
            return;
        }

        var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
        if (status != PermissionStatus.Granted)
        {
            await Shell.Current.DisplayAlert("Permission Required", "Location permission is required for GPS tracking.", "OK");
            return;
        }

        _points.Clear();
        _sequence = 0;
        _activeRoute = await _api.CreateRouteAsync(RouteName, null, SelectedMode, DateTime.UtcNow);

        if (_activeRoute == null)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to create route. Check your connection.", "OK");
            return;
        }

        IsTracking = true;
        TrackingStatus = $"Tracking: {RouteName}";
        _trackingCts = new CancellationTokenSource();
        _ = TrackLocationAsync(_trackingCts.Token);
    }

    [RelayCommand]
    private async Task StopTrackingAsync()
    {
        _trackingCts?.Cancel();
        IsTracking = false;

        if (_activeRoute != null && _points.Count > 0)
        {
            var totalDist = CalculateTotalDistance(_points);
            var dur = (_points.Last().RecordedAt - _points.First().RecordedAt).TotalSeconds;
            await _api.CompleteRouteAsync(_activeRoute.Id, DateTime.UtcNow, totalDist, dur);
            Distance = FormatDistance(totalDist);
            TrackingStatus = $"Route saved! {Distance}";
        }
        else
        {
            TrackingStatus = "Tracking stopped";
        }
        RouteName = string.Empty;
        _activeRoute = null;
    }

    private async Task TrackLocationAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var pointBatch = new List<object>();
        var lastBatchTime = DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(10)
                }, cancellationToken);

                if (location != null)
                {
                    CurrentLatitude = location.Latitude;
                    CurrentLongitude = location.Longitude;

                    var point = new RoutePoint
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Altitude = location.Altitude,
                        SpeedMps = location.Speed,
                        Sequence = _sequence++,
                        RecordedAt = DateTime.UtcNow
                    };
                    _points.Add(point);
                    PointCount = _points.Count;

                    pointBatch.Add(new
                    {
                        latitude = point.Latitude,
                        longitude = point.Longitude,
                        altitude = point.Altitude,
                        speedMps = point.SpeedMps,
                        sequence = point.Sequence,
                        recordedAt = point.RecordedAt
                    });

                    // Batch upload every 10 points or 30 seconds
                    if (pointBatch.Count >= 10 || (DateTime.UtcNow - lastBatchTime).TotalSeconds >= 30)
                    {
                        if (_activeRoute != null)
                        {
                            await _api.AddRoutePointsBatchAsync(_activeRoute.Id, new List<object>(pointBatch));
                            pointBatch.Clear();
                            lastBatchTime = DateTime.UtcNow;
                        }
                    }

                    Distance = FormatDistance(CalculateTotalDistance(_points));
                    Duration = FormatDuration((DateTime.UtcNow - startTime).TotalSeconds);
                }
            }
            catch (OperationCanceledException) { break; }
            catch { /* continue tracking */ }

            await Task.Delay(3000, cancellationToken).ContinueWith(_ => { });
        }

        // Upload remaining batch
        if (pointBatch.Count > 0 && _activeRoute != null)
        {
            await _api.AddRoutePointsBatchAsync(_activeRoute.Id, pointBatch);
        }
    }

    private static double CalculateTotalDistance(List<RoutePoint> points)
    {
        double total = 0;
        for (int i = 1; i < points.Count; i++)
            total += Haversine(points[i - 1].Latitude, points[i - 1].Longitude, points[i].Latitude, points[i].Longitude);
        return total;
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
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
