using System.Collections.Generic;
using System.Linq;

namespace AZ.GeoPolygon
{
    public static class AZGeoExtensions
    {
        public static List<AZGeoPoint> ToGcj02(this List<AZGeoPoint> points)
        {
            return points.Select(p => AZWGSConverter.ToGcj02Point(p.Lat, p.Lon)).ToList();
        }

        public static List<AZGeoPoint> ToWgs84(this List<AZGeoPoint> points)
        {
            return points.Select(p => AZWGSConverter.ToWgs84Point(p.Lat, p.Lon)).ToList();
        }

        public static AZGeoPoint ToGcj02(this AZGeoPoint point)
        {
            return AZWGSConverter.ToGcj02Point(point.Lat, point.Lon);
        }
        public static AZGeoPoint ToWgs84(this AZGeoPoint point)
        {
            return AZWGSConverter.ToWgs84Point(point.Lat, point.Lon);
        }

    }
}
