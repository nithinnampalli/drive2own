namespace Drive2Own.Mobile.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public User User { get; set; } = new();
}

public class LocationPin
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? IconColor { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoutePoint
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? SpeedMps { get; set; }
    public int Sequence { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class TripRoute
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TravelMode { get; set; } = "Car";
    public double TotalDistanceMeters { get; set; }
    public double TotalDurationSeconds { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RoutePoint> Points { get; set; } = new();
}

public class DashboardData
{
    public int TotalRoutes { get; set; }
    public double TotalDistanceMeters { get; set; }
    public double TotalDurationSeconds { get; set; }
    public int TotalPins { get; set; }
    public List<RouteMetric> RecentRoutes { get; set; } = new();
    public Dictionary<string, double> DistanceByMode { get; set; } = new();
}

public class RouteMetric
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TravelMode { get; set; } = string.Empty;
    public double DistanceMeters { get; set; }
    public double DurationSeconds { get; set; }
    public DateTime StartedAt { get; set; }
}

public class RouteShare
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public string ShareToken { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string RouteUrl { get; set; } = string.Empty;
}
