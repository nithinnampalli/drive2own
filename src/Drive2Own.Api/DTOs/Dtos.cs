namespace Drive2Own.Api.DTOs;

public record RegisterRequest(string Username, string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);
public record UpdateProfileRequest(string DisplayName, string? AvatarUrl);

public record UserDto(int Id, string Username, string Email, string DisplayName, string? AvatarUrl, DateTime CreatedAt);

public record LocationPinDto(int Id, int UserId, string Title, string? Description, double Latitude, double Longitude, string? IconColor, DateTime CreatedAt);
public record CreateLocationPinRequest(string Title, string? Description, double Latitude, double Longitude, string? IconColor);
public record UpdateLocationPinRequest(string Title, string? Description, string? IconColor);

public record RouteDto(int Id, int UserId, string Name, string? Description, string TravelMode, double TotalDistanceMeters, double TotalDurationSeconds, DateTime StartedAt, DateTime? CompletedAt, bool IsCompleted, DateTime CreatedAt, List<RoutePointDto> Points);
public record RoutePointDto(int Id, double Latitude, double Longitude, double? Altitude, double? SpeedMps, int Sequence, DateTime RecordedAt);
public record CreateRouteRequest(string Name, string? Description, string TravelMode, DateTime StartedAt);
public record CompleteRouteRequest(DateTime CompletedAt, double TotalDistanceMeters, double TotalDurationSeconds);
public record AddRoutePointRequest(double Latitude, double Longitude, double? Altitude, double? SpeedMps, int Sequence, DateTime RecordedAt);

public record DashboardDto(
    int TotalRoutes,
    double TotalDistanceMeters,
    double TotalDurationSeconds,
    int TotalPins,
    List<RouteMetricDto> RecentRoutes,
    Dictionary<string, double> DistanceByMode
);
public record RouteMetricDto(int Id, string Name, string TravelMode, double DistanceMeters, double DurationSeconds, DateTime StartedAt);

public record RouteShareDto(int Id, int RouteId, string ShareToken, bool IsPublic, DateTime CreatedAt, DateTime? ExpiresAt, string RouteUrl);
public record CreateRouteShareRequest(bool IsPublic, int? ExpiresInHours);
public record SharedRouteDto(RouteDto Route, string SharedBy, DateTime SharedAt);
