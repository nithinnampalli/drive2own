namespace Drive2Own.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LocationPin> LocationPins { get; set; } = new List<LocationPin>();
    public ICollection<Route> Routes { get; set; } = new List<Route>();
    public ICollection<RouteShare> SharedRoutes { get; set; } = new List<RouteShare>();
}
