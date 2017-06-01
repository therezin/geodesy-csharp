﻿using System;

namespace geodesy_csharp
{
    public class LatLon
    {
        public LatLon(double latitude, double longitude, Datum datum)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Datum = datum;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Datum Datum { get; set; }
        

        public LatLon ConvertDatum(Datum toDatum)
        {
            LatLon OldLatLon = this;
            double[] Transform = toDatum.Transform;
            bool UsingWgs84 = false;

            if (OldLatLon.Datum == Datum.WGS84)
            {
                // converting from WGS 84
                Transform = toDatum.Transform;
                UsingWgs84 = true;
            }
            if (toDatum == Datum.WGS84)
            {
                // converting to WGS 84; use inverse transform (don't overwrite original!)
                UsingWgs84 = true;
                Transform = new double[7];
                for (var p = 0; p < 7; p++)
                {
                    Transform[p] = -OldLatLon.Datum.Transform[p];
                }
            }
            if (!UsingWgs84)
            {
                // neither this.datum nor toDatum are WGS84: convert this to WGS84 first
                OldLatLon = ConvertDatum(Datum.WGS84);
            }

            var OldCartesian = OldLatLon.ToCartesian();                // convert polar to cartesian...
            var NewCartesian = OldCartesian.ApplyTransform(Transform); // ...apply transform...
            var NewLatLon = NewCartesian.ToLatLonE(toDatum);           // ...and convert cartesian to polar

            return NewLatLon;

        }

        private Vector3D ToCartesian()
        {
            var φ = this.Latitude.ToRadians();
            var λ = this.Longitude.ToRadians();
            var h = 0; // height above ellipsoid - not currently used
            var a = this.Datum.Ellipsoid.Major;
            var f = this.Datum.Ellipsoid.Flattening;

            var sinφ = Math.Sin(φ);
            var cosφ = Math.Cos(φ);
            var sinλ = Math.Sin(λ);
            var cosλ = Math.Cos(λ);

            var eSq = 2 * f - f * f;                      // 1st eccentricity squared ≡ (a²-b²)/a²
            var ν = a / Math.Sqrt(1 - eSq * sinφ * sinφ); // radius of curvature in prime vertical

            var x = (ν + h) * cosφ * cosλ;
            var y = (ν + h) * cosφ * sinλ;
            var z = (ν * (1 - eSq) + h) * sinφ;

            var point = new Vector3D(x, y, z);

            return point;
        }
    }    
}