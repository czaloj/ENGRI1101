using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ORLabs.Graphics
{
    public static class GoogleMap
    {
        public static Texture2D getMapTexture(GraphicsDevice g, ImageRequest req)
        {
            //The Texture To Be Returned
            Texture2D t = null;

            try
            {
                //Build The Texture From A Stream
                using (WebClient web = new WebClient())
                {
                    using (MemoryStream s = new MemoryStream(web.DownloadData(req.URL)))
                    {
                        t = Texture2D.FromStream(g, s);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return t;
        }
        public static System.Drawing.Image getMapImage(ImageRequest req)
        {
            //The Texture To Be Returned
            System.Drawing.Image t = null;

            try
            {
                //Build The Texture From A Stream
                using (WebClient web = new WebClient())
                {
                    using (MemoryStream s = new MemoryStream(web.DownloadData(req.URL)))
                    {
                        t = System.Drawing.Image.FromStream(s);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return t;
        }
    }

    public struct ImageRequest
    {
        #region Static
        public const string URLFormat = @"http://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom={2}&size={3}x{4}&scale={5}&maptype={6}&sensor=false";

        private static readonly string[] MapTypes = 
            {
                "roadmap",
                "satellite",
                "hybrid",
                "terrain"
            };
        public const int NumMapTypes = 4;
        public const int MT_RoadMap = 0;
        public const int MT_Satellite = 1;
        public const int MT_Hybrid = 2;
        public const int MT_Terrain = 3;

        public const decimal MaxLatitude = 90m;
        public const decimal MinLatitude = -90m;
        public const decimal MaxLongitude = 180m;
        public const decimal MinLongitude = -180m;

        public const int MaxZoom = 26;
        public const int MinZoom = 1;
        #endregion

        public string URL { get { return string.Format(URLFormat, Latitude, Longitude, Zoom, PWidth, PHeight, Scale, MapType); ; } }

        public decimal Latitude, Longitude;
        public int Zoom, PWidth, PHeight, Scale;

        public string MapType;

        public ImageRequest(
            decimal lat, decimal lon, int zoom,
            int pw, int ph, int scale,
            int mapType
            )
        {
            if (lat < MinLatitude || lat > MaxLatitude) { throw new ArgumentException("Latitude Must Be Between -90 - 90"); }
            Latitude = lat;
            if (lon < MinLongitude || lon > MaxLongitude) { throw new ArgumentException("Longitude Must Be Between -180 - 180"); }
            Longitude = lon;
            if (zoom < MinZoom || zoom > MaxZoom) { throw new ArgumentException("Zoom Must Be Between 1 - 26"); }
            Zoom = zoom;
            PWidth = pw;
            PHeight = ph;
            if (scale < 1 || scale > 2) { throw new ArgumentException("Scale Must Be Either 0 or 1"); }
            Scale = scale;
            if (mapType < 0 || mapType > 3) { throw new ArgumentException("Map Type Must Be Between 0 - 3"); }
            MapType = MapTypes[mapType];
        }
        public ImageRequest(
            decimal lat, decimal lon, int zoom,
            int pw, int ph, int scale
            )
            : this(lat, lon, zoom, pw, ph, scale, MT_RoadMap)
        { }
        public ImageRequest(
            decimal lat, decimal lon, int zoom,
            int pw, int ph
            )
            : this(lat, lon, zoom, pw, ph, 1)
        { }
        public ImageRequest(
            decimal lat, decimal lon, int pw, int ph
            )
            : this(lat, lon, 10, pw, ph)
        { }
        public ImageRequest(
            decimal lat, decimal lon
            )
            : this(lat, lon, 512, 512)
        { }

    }
    public struct GeoCodeRequest
    {
        public const string URLFormat = @"http://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false";

        public string Address;
        private string addressF;

        public string URL { get { return string.Format(URLFormat, addressF); } }

        public GeoCodeRequest(string addr)
        {
            if (string.IsNullOrWhiteSpace(addr)) { throw new ArgumentException("Must Have An Address Specified"); }
            Address = addr;
            addressF = System.Text.RegularExpressions.Regex.Replace(Address, @"\s+", "+");
        }
    }
    public struct GeoCodeResults
    {
        public LatLon[] Results;
        public LatLon this[int i] { get { return Results[i]; } }

        public decimal Latitude { get { return Results[0].Latitude; } }
        public decimal Longitude { get { return Results[0].Longitude; } }

        public static bool fromRequest(GeoCodeRequest req, out GeoCodeResults res)
        {
            res = new GeoCodeResults();
            try
            {
                XmlTextReader xmlr = new XmlTextReader(req.URL);

                if (!xmlr.ReadToFollowing("GeocodeResponse")) { return false; }

                if (!xmlr.ReadToFollowing("status")) { return false; }
                string sOK = xmlr.ReadElementContentAsString();
                if (!sOK.Equals("OK")) { return false; }

                LinkedList<LatLon> lr = new LinkedList<LatLon>();
                LatLon ll = new LatLon(0, 0);
                while (xmlr.ReadToFollowing("result"))
                {
                    if (!xmlr.ReadToFollowing("geometry")) { continue; }
                    if (!xmlr.ReadToFollowing("location")) { continue; }

                    if (!xmlr.ReadToFollowing("lat")) { continue; }
                    if (!decimal.TryParse(xmlr.ReadElementContentAsString(), out ll.Latitude)) { continue; }

                    if (!xmlr.ReadToFollowing("lng")) { continue; }
                    if (!decimal.TryParse(xmlr.ReadElementContentAsString(), out ll.Longitude)) { continue; }

                    lr.AddLast(ll);
                }
                if (lr.Count < 1) { return false; }
                res.Results = lr.ToArray();
                return true;
            }
            catch (Exception) { return false; }
        }
    }
    public struct LatLon
    {
        public decimal Latitude, Longitude;

        public LatLon(decimal lat, decimal lon)
        {
            Latitude = lat;
            Longitude = lon;
        }
    }
}
