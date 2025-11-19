namespace TrafficTrack.Core.Models;

public class BoundingBox
{
    public GeoCoordinate TopLeft { get; set; }
    public GeoCoordinate BottomRight { get; set; }

    public BoundingBox(GeoCoordinate topLeft, GeoCoordinate bottomRight)
    {
        TopLeft = topLeft;
        BottomRight = bottomRight;
    }

    public BoundingBox(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude)
    {
        TopLeft = new GeoCoordinate(maxLatitude, minLongitude);
        BottomRight = new GeoCoordinate(minLatitude, maxLongitude);
    }
}
