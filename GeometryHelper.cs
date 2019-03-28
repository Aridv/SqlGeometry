using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlTypes;
using System.IO;

public class GeometryHelper
{
    private const string POLYGON_START = "POLYGON((";
    private const string POLYGON_END = "))";
    private const int SRID = 4326;

    public GeometryHelper()
    {

    }

    /// <summary>
    /// SqlGeometry expects a Well Known Text to create the geometry object: POLYGON(('latitude longitude, latitude longitude...'))
    ///     For more info: https://en.wikipedia.org/wiki/Well-known_text_representation_of_geometry
    /// </summary>
    /// <param name="coordinates">Json Array with coordinates from a Polygon</param>
    /// <returns>Returns a Well Known Text valid format for Polygon object</returns>
    public static string GetWKTPolygon(string coordinates)
    {
        try
        {
            JArray jsonArray = JsonConvert.DeserializeObject<JArray>(coordinates);
            string polygon = POLYGON_START;

            foreach (JObject obj in jsonArray.Children())
            {
                polygon += obj.Value<double>("lat").ToString("G17").Replace(',', '.') + " " + obj.Value<double>("lng").ToString("G17").Replace(',', '.') + ", ";
            }

            // In order to obtain a valid format the first point must be the same than the last, if the json doesn't come with it, we add it
            JToken firstPoint = jsonArray.First;
            JToken lastPoint = jsonArray.Last;

            string latFrstPnt = firstPoint.Value<double>("lat").ToString("G17").Replace(',', '.');
            string lngFrstPnt = firstPoint.Value<double>("lng").ToString("G17").Replace(',', '.');
            string latLstPnt = lastPoint.Value<double>("lat").ToString("G17").Replace(',', '.');
            string lngLstPnt = lastPoint.Value<double>("lng").ToString("G17").Replace(',', '.');

            if (!latFrstPnt.Equals(latLstPnt) && !lngFrstPnt.Equals(lngLstPnt))
            {
                polygon += latFrstPnt + " " + lngFrstPnt;
            }

            // If the json array of coordinates comes from a database as binary, it will have the same point at the beginning and the end, therefore we will need to delete the last character we added when creating the WKT valid format
            polygon = polygon.Trim();

            if (polygon.EndsWith(","))
            {
                polygon = polygon.Remove(polygon.Length - 1);
            }

            return polygon += POLYGON_END;
        } catch (Exception ex)
        {
            //LogManager.AddLogError("");
        }

        return null;
    }

    /// <summary>
    /// We create the binary object to store in the database, it can be both data types (varbinary(MAX) or geometry)
    /// </summary>
    /// <param name="wkt">The Well Known Text needed to create the binary</param>
    /// <returns></returns>
    public static byte[] GetBinaryFromCoordinates(string wkt)
    {
        SqlChars sqlChars = new SqlChars(wkt);
        SqlGeometry sqlGeometry = SqlGeometry.STPolyFromText(sqlChars, SRID);
        return sqlGeometry.Serialize().Value;
    }

    /// <summary>
    /// We obtain the Json Array of coordinates from a binary stored in the database
    /// </summary>
    /// <param name="binary">Byte array from the database</param>
    /// <returns></returns>
    public static string GetCoordinatesFromBinary(byte[] binary)
    {
        SqlBytes sqlBytes = new SqlBytes(binary);
        BinaryReader br = new BinaryReader(sqlBytes.Stream);

        SqlGeometry sqlGeometry = new SqlGeometry();
        sqlGeometry.Read(br);

        JArray array = new JArray();
        for (int i = 1; i <= sqlGeometry.STNumPoints(); i++)
        {
            SqlGeometry point = sqlGeometry.STPointN(i);
            double lat = point.STX.Value;
            double lng = point.STY.Value;

            JObject obj = new JObject();
            obj.Add("lat", lat);
            obj.Add("lng", lng);
            array.Add(obj);
        }

        return JsonConvert.SerializeObject(array, Formatting.None);
    }
}