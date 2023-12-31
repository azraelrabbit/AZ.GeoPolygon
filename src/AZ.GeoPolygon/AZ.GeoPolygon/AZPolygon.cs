using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace AZ.GeoPolygon
{
    public interface IAZPolygon
    {
        bool Contains(AZGeoPoint location);
    }

    public class AZPolygon : IAZPolygon
    {
        //private readonly List<AZGeoPoint> _points;

        private readonly List<AZGeoPoint> _pointsWithClosure;

        private double minX, maxX, minY, maxY;

        //private  Memory<AZGeoPoint> _pointsWithClosureM;

        public AZPolygon(List<AZGeoPoint> points)
        {
            _pointsWithClosure = PolygonPointsWithClosure(points);
            InitializeBoundingBox();
        }

        private void InitializeBoundingBox()
        {
            minX = double.MaxValue;
            maxX = double.MinValue;
            minY = double.MaxValue;
            maxY = double.MinValue;

            foreach (var vertex in _pointsWithClosure)
            {
                minX = Math.Min(minX, vertex.Lon);
                maxX = Math.Max(maxX, vertex.Lon);
                minY = Math.Min(minY, vertex.Lat);
                maxY = Math.Max(maxY, vertex.Lat);
            }
        }

        private bool IsPointInsideBoundingBox(AZGeoPoint point)
        {
            return point.Lon >= minX && point.Lon <= maxX && point.Lat >= minY && point.Lat <= maxY;
        }


        ~AZPolygon()
        {
            _pointsWithClosure.Clear();
            //_pointsWithClosure = null;
        }

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool Contains(AZGeoPoint location)
        {
            //AZGeoPoint[] polygonPointsWithClosure = PolygonPointsWithClosure();

            //first check the location does inside the polygon bound(rect),to improve performance
            if (!IsPointInsideBoundingBox(location))
                return false;

            int windingNumber = 0;

            int len = _pointsWithClosure.Count - 1;

            var _pSpan = CollectionsMarshal.AsSpan<AZGeoPoint>(_pointsWithClosure);


            //for (int pointIndex = 0; pointIndex < len; pointIndex++)
            //{
            //    //AZEdge edge = new AZEdge(_pointsWithClosure.Span[pointIndex], _pointsWithClosure.Span[pointIndex + 1]);

            //    AZEdge edge = new AZEdge(_pSpan[pointIndex], _pSpan[pointIndex + 1]);
            //    windingNumber += AscendingIntersection(location, edge);

            //    windingNumber -= DescendingIntersection(location, edge);
            //}
            // do Performance optimization
            for (int pointIndex = 0; pointIndex < len; pointIndex += 4)
            {
                //AZEdge edge = new AZEdge(_pointsWithClosure.Span[pointIndex], _pointsWithClosure.Span[pointIndex + 1]);

                if (pointIndex + 4 < len)
                {
                    AZEdge edge = new AZEdge(_pSpan[pointIndex], _pSpan[pointIndex + 1]);
                    windingNumber += AscendingIntersection(location, edge);
                    windingNumber -= DescendingIntersection(location, edge);

                    AZEdge edge1 = new AZEdge(_pSpan[pointIndex + 1], _pSpan[pointIndex + 1 + 1]);
                    windingNumber += AscendingIntersection(location, edge1);
                    windingNumber -= DescendingIntersection(location, edge1);

                    AZEdge edge2 = new AZEdge(_pSpan[pointIndex + 2], _pSpan[pointIndex + 2 + 1]);
                    windingNumber += AscendingIntersection(location, edge2);
                    windingNumber -= DescendingIntersection(location, edge2);

                    AZEdge edge3 = new AZEdge(_pSpan[pointIndex + 3], _pSpan[pointIndex + 3 + 1]);
                    windingNumber += AscendingIntersection(location, edge3);
                    windingNumber -= DescendingIntersection(location, edge3);
                }
                else
                {
                    AZEdge edge = new AZEdge(_pSpan[pointIndex], _pSpan[pointIndex + 1]);
                    windingNumber += AscendingIntersection(location, edge);
                    windingNumber -= DescendingIntersection(location, edge);

                    if (pointIndex + 1 < len)
                    {
                        AZEdge edge1 = new AZEdge(_pSpan[pointIndex + 1], _pSpan[pointIndex + 1 + 1]);
                        windingNumber += AscendingIntersection(location, edge1);
                        windingNumber -= DescendingIntersection(location, edge1);
                    }
                    if (pointIndex + 2 < len)
                    {
                        AZEdge edge2 = new AZEdge(_pSpan[pointIndex + 2], _pSpan[pointIndex + 2 + 1]);
                        windingNumber += AscendingIntersection(location, edge2);
                        windingNumber -= DescendingIntersection(location, edge2);
                    }
                    if (pointIndex + 3 < len)
                    {
                        AZEdge edge3 = new AZEdge(_pSpan[pointIndex + 3], _pSpan[pointIndex + 3 + 1]);
                        windingNumber += AscendingIntersection(location, edge3);
                        windingNumber -= DescendingIntersection(location, edge3);
                    }
                }
            }

            return windingNumber != 0;
        }

        public bool ContainsSlow(AZGeoPoint location)
        {
            //AZGeoPoint[] polygonPointsWithClosure = PolygonPointsWithClosure();

            //first check the location does inside the polygon rectangular bounds,to improve performance
            if (!IsPointInsideBoundingBox(location))
                return false;

            int windingNumber = 0;

            int len = _pointsWithClosure.Count - 1;

            var _pSpan = CollectionsMarshal.AsSpan<AZGeoPoint>(_pointsWithClosure);


            for (int pointIndex = 0; pointIndex < len; pointIndex++)
            {
                //AZEdge edge = new AZEdge(_pointsWithClosure.Span[pointIndex], _pointsWithClosure.Span[pointIndex + 1]);

                AZEdge edge = new AZEdge(_pSpan[pointIndex], _pSpan[pointIndex + 1]);
                windingNumber += AscendingIntersection(location, edge);

                windingNumber -= DescendingIntersection(location, edge);
            }
          


            return windingNumber != 0;
        }

        //private AZGeoPoint[] PolygonPointsWithClosure(List<AZGeoPoint> points)
        //{
        //    // _points should remain immutable, thus creation of a closed point set (starting point repeated)
        //    //    return new List<AZGeoPoint>(_points)
        //    //{
        //    //    new AZGeoPoint(_points[0].Lat, _points[0].Lon)
        //    //}.ToArray();

        //    var start = points[0];
        //    var end = points[points.Count - 1];

        //    var ret = new List<AZGeoPoint>(points);

        //    // should remain immutable, thus creation of a closed point set (starting point repeated)
        //    if (start.Lat != end.Lat || start.Lon != end.Lon)
        //    {
        //        ret.Add(new AZGeoPoint(start.Lat, start.Lon));
        //    }

        //    return ret.ToArray();

        //}
        private List<AZGeoPoint> PolygonPointsWithClosure(List<AZGeoPoint> points)
        {
            // _points should remain immutable, thus creation of a closed point set (starting point repeated)
            //    return new List<AZGeoPoint>(_points)
            //{
            //    new AZGeoPoint(_points[0].Lat, _points[0].Lon)
            //}.ToArray();

            var start = points[0];
            var end = points[points.Count - 1];

            var ret = new List<AZGeoPoint>(points);

            // should remain immutable, thus creation of a closed point set (starting point repeated)
            if (start.Lat != end.Lat || start.Lon != end.Lon)
            {
                ret.Add(new AZGeoPoint(start.Lat, start.Lon));
            }

            return ret;

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
