using System.Net.Http.Json;
using System.Text.Json;
using Drive2Own.Mobile.Models;

namespace Drive2Own.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string BaseUrl = "http://localhost:5000/api";

    public ApiService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    // Auth
    public Task<AuthResponse?> RegisterAsync(string username, string email, string password, string displayName) =>
        PostAsync<AuthResponse>("/auth/register", new { username, email, password, displayName });

    public Task<AuthResponse?> LoginAsync(string email, string password) =>
        PostAsync<AuthResponse>("/auth/login", new { email, password });

    // Dashboard
    public Task<DashboardData?> GetDashboardAsync() =>
        GetAsync<DashboardData>("/dashboard");

    // Location Pins
    public Task<List<LocationPin>?> GetLocationPinsAsync() =>
        GetAsync<List<LocationPin>>("/locationpins");

    public Task<LocationPin?> CreateLocationPinAsync(string title, string? description, double lat, double lng, string? iconColor = null) =>
        PostAsync<LocationPin>("/locationpins", new { title, description, latitude = lat, longitude = lng, iconColor });

    public Task DeleteLocationPinAsync(int id) =>
        _httpClient.DeleteAsync($"/locationpins/{id}");

    // Routes
    public Task<List<TripRoute>?> GetRoutesAsync() =>
        GetAsync<List<TripRoute>>("/routes");

    public Task<TripRoute?> CreateRouteAsync(string name, string? description, string travelMode, DateTime startedAt) =>
        PostAsync<TripRoute>("/routes", new { name, description, travelMode, startedAt });

    public Task<TripRoute?> CompleteRouteAsync(int id, DateTime completedAt, double totalDistanceMeters, double totalDurationSeconds) =>
        PostAsync<TripRoute>($"/routes/{id}/complete", new { completedAt, totalDistanceMeters, totalDurationSeconds });

    public Task AddRoutePointsBatchAsync(int routeId, List<object> points) =>
        PostAsync<object>($"/routes/{routeId}/points/batch", points);

    public Task DeleteRouteAsync(int id) =>
        _httpClient.DeleteAsync($"/routes/{id}");

    // Shares
    public Task<RouteShare?> ShareRouteAsync(int routeId, bool isPublic, int? expiresInHours = null) =>
        PostAsync<RouteShare>($"/shares/routes/{routeId}", new { isPublic, expiresInHours });

    // Private helpers
    private async Task<T?> GetAsync<T>(string path)
    {
        var response = await _httpClient.GetAsync(path);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T?> PostAsync<T>(string path, object body)
    {
        var response = await _httpClient.PostAsJsonAsync(path, body);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }
}
