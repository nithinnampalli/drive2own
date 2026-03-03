namespace Drive2Own.Api.Models;

public class RoutePoint
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? SpeedMps { get; set; }
    public int Sequence { get; set; }
    public DateTime RecordedAt { get; set; }

    public Route Route { get; set; } = null!;
}
