namespace AZ.GeoPolygon
{
    public class AZGeoPoint
    {
        public double Lon { get; set; }

        public double Lat { get; set; }

        public AZGeoPoint(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
    }
}