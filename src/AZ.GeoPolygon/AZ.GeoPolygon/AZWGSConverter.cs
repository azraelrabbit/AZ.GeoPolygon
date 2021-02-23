using System;

namespace AZ.GeoPolygon
{


    public class AZWGSConverter
    {
        private static double a = 6378245.0;
        private static double pi = 3.1415926535897932384626;
        private static double ee = 0.00669342162296594323;



        /// <summary>
        /// 计算地球上任意两点(经纬度)距离
        /// </summary>
        /// <param name="lat1">第一点纬度</param>
        /// <param name="lng1">第一点经度</param>
        /// <param name="lat2">第二点纬度</param>
        /// <param name="lng2">第二点经度</param>
        /// <returns>返回距离 单位：米</returns>
        public static double Distance(double lat1, double lng1, double lat2, double lng2)
        {
            double a, b, R;
            R = 6378137; // 地球半径
            lat1 = lat1 * Math.PI / 180.0;
            lat2 = lat2 * Math.PI / 180.0;
            a = lat1 - lat2;
            b = (lng1 - lng2) * Math.PI / 180.0;
            double d;
            double sa2, sb2;
            sa2 = Math.Sin(a / 2.0);
            sb2 = Math.Sin(b / 2.0);
            d = 2 * R * Math.Asin(Math.Sqrt(sa2 * sa2 + Math.Cos(lat1) * Math.Cos(lat2) * sb2 * sb2));
            return d;
        }
 

        /// <summary>
        /// WGS-84 to GCJ-02  
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static AZGeoPoint ToGcj02Point(double lat, double lon)
        {
            double[] dev = CalDev(lat, lon);
            double retLat = lat + dev[0];
            double retLon = lon + dev[1];
 
            return new AZGeoPoint(retLat, retLon);
        }
        
 
        /// <summary>
        /// WGS-84 to GCJ-02
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <param name="scale">保留小数位数</param>
        /// <returns></returns>
        public static AZGeoPoint ToGcj02Point(double lat, double lon, int scale)
        {
            double[] dev = CalDev(lat, lon);
            double retLat = lat + dev[0];
            double retLon = lon + dev[1];


            var rlat = RoundDown(retLat, scale);
            var rlon = RoundDown(retLon, scale);

            return new AZGeoPoint(rlat, rlon);
           
        }

        public static double RoundDown(double d, int n)
        {
            string strDecimal = d.ToString();
            int index = strDecimal.IndexOf(".");
            if (index == -1 || strDecimal.Length < index + n + 1)
            {
                strDecimal = string.Format("{0:F" + n + "}", d);
            }
            else
            {
                int length = index;
                if (n != 0)
                {
                    length = index + n + 1;
                }
                strDecimal = strDecimal.Substring(0, length);
            }
            return double.Parse(strDecimal);
        }

        public static decimal RoundDown(decimal d, int n)
        {
            string strDecimal = d.ToString();
            int index = strDecimal.IndexOf(".");
            if (index == -1 || strDecimal.Length < index + n + 1)
            {
                strDecimal = string.Format("{0:F" + n + "}", d);
            }
            else
            {
                int length = index;
                if (n != 0)
                {
                    length = index + n + 1;
                }
                strDecimal = strDecimal.Substring(0, length);
            }
            return Decimal.Parse(strDecimal);
        }

 

        /// <summary>
        /// GCJ-02 to WGS-84
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <returns></returns>
        public static AZGeoPoint ToWgs84Point(double lat, double lon)
        {
            double[] dev = CalDev(lat, lon);
            double retLat = lat - dev[0];
            double retLon = lon - dev[1];
            dev = CalDev(retLat, retLon);
            retLat = lat - dev[0];
            retLon = lon - dev[1];
            //return new double[] { retLat, retLon };
            return new AZGeoPoint(retLat, retLon);
        }

        private static double[] CalDev(double wgLat, double wgLon)
        {
            if (IsOutOfChina(wgLat, wgLon))
            {
                return new double[] { 0, 0 };
            }
            double dLat = CalLat(wgLon - 105.0, wgLat - 35.0);
            double dLon = CalLon(wgLon - 105.0, wgLat - 35.0);
            double radLat = wgLat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            return new double[] { dLat, dLon };
        }

        private static bool IsOutOfChina(double lat, double lon)
        {
            if (lon < 72.004d || lon > 137.8347d)
                return true;
            if (lat < 0.8293d || lat > 55.8271d)
                return true;
            return false;
        }

        private static double CalLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        private static double CalLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }
    }

}
