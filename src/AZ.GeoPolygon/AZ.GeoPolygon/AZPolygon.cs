using System.Collections.Generic;

namespace AZ.GeoPolygon
{
    public interface IAZPolygon
    {
        bool Contains(AZGeoPoint location);
    }

    public class AZPolygon : IAZPolygon
    {
        private readonly List<AZGeoPoint> _points;

        public AZPolygon(List<AZGeoPoint> points)
        {
            _points = points;
        }

        ~AZPolygon()
        {
            _points.Clear();
        }

        public bool Contains(AZGeoPoint location)
        {
            AZGeoPoint[] polygonPointsWithClosure = PolygonPointsWithClosure();

            int windingNumber = 0;

            for (int pointIndex = 0; pointIndex < polygonPointsWithClosure.Length - 1; pointIndex++)
            {
                AZEdge edge = new AZEdge(polygonPointsWithClosure[pointIndex], polygonPointsWithClosure[pointIndex + 1]);
                windingNumber += AscendingIntersection(location, edge);
                windingNumber -= DescendingIntersection(location, edge);
            }

            return windingNumber != 0;
        }

        private AZGeoPoint[] PolygonPointsWithClosure()
        {
            // _points should remain immutable, thus creation of a closed point set (starting point repeated)
            return new List<AZGeoPoint>(_points)
        {
            new AZGeoPoint(_points[0].Lat, _points[0].Lon)
        }.ToArray();
        }

        private static int AscendingIntersection(AZGeoPoint location, AZEdge edge)
        {
            if (!edge.AscendingRelativeTo(location)) { return 0; }

            if (!edge.LocationInRange(location, Orientation.Ascending)) { return 0; }

            return Wind(location, edge, Position.Left);
        }

        private static int DescendingIntersection(AZGeoPoint location, AZEdge edge)
        {
            if (edge.AscendingRelativeTo(location)) { return 0; }

            if (!edge.LocationInRange(location, Orientation.Descending)) { return 0; }

            return Wind(location, edge, Position.Right);
        }

        private static int Wind(AZGeoPoint location, AZEdge edge, Position position)
        {
            if (edge.RelativePositionOf(location) != position) { return 0; }

            return 1;
        }

        private class AZEdge
        {
            private readonly AZGeoPoint _startPoint;
            private readonly AZGeoPoint _endPoint;

            public AZEdge(AZGeoPoint startPoint, AZGeoPoint endPoint)
            {
                _startPoint = startPoint;
                _endPoint = endPoint;
            }

            public Position RelativePositionOf(AZGeoPoint location)
            {
                double positionCalculation =
                    (_endPoint.Lon - _startPoint.Lon) * (location.Lat - _startPoint.Lat) -
                    (location.Lon - _startPoint.Lon) * (_endPoint.Lat - _startPoint.Lat);

                if (positionCalculation > 0) { return Position.Left; }

                if (positionCalculation < 0) { return Position.Right; }

                return Position.Center;
            }

            public bool AscendingRelativeTo(AZGeoPoint location)
            {
                return _startPoint.Lat <= location.Lat;
            }

            public bool LocationInRange(AZGeoPoint location, Orientation orientation)
            {
                if (orientation == Orientation.Ascending) return _endPoint.Lat > location.Lat;

                return _endPoint.Lat <= location.Lat;
            }
        }

        private enum Position
        {
            Left,
            Right,
            Center
        }

        private enum Orientation
        {
            Ascending,
            Descending
        }
 
    }
}
