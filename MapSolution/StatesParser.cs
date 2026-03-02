using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace MapSolution
{
    public class GeoPoint
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
        public GeoPoint(double lon, double lat) { Lon = lon; Lat = lat; }
    }

    public class StateGeometry
    {
        public string Code { get; set; } = "";
        public List<List<GeoPoint>> Polygons { get; set; } = new List<List<GeoPoint>>();
    }

    public static class StatesParser
    {
        public static Dictionary<string, StateGeometry> Parse(string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            var result = new Dictionary<string, StateGeometry>();

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;

                    foreach (JsonProperty stateProperty in root.EnumerateObject())
                    {
                        string stateCode = stateProperty.Name;
                        var state = new StateGeometry { Code = stateCode };
                        
                        foreach (JsonElement polygon in stateProperty.Value.EnumerateArray())
                        {
                            
                            int ringIndex = 0;
                            foreach (JsonElement ring in polygon.EnumerateArray())
                            {
                                if (ringIndex == 0 && ring.ValueKind == JsonValueKind.Array)
                                {
                                    var points = new List<GeoPoint>();

                                    foreach (JsonElement coord in ring.EnumerateArray())
                                    {
                                        if (coord.ValueKind == JsonValueKind.Array && coord.GetArrayLength() >= 2)
                                        {
                                            try
                                            {
                                                double lon = coord[0].GetDouble();
                                                double lat = coord[1].GetDouble();
                                                points.Add(new GeoPoint(lon, lat));
                                            }
                                            catch
                                            {
                                                
                                            }
                                        }
                                    }

                                    if (points.Count >= 3)
                                    {
                                        state.Polygons.Add(points);
                                    }
                                }
                                ringIndex++;
                            }
                        }

                        if (state.Polygons.Count > 0)
                        {
                            result[stateCode] = state;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing JSON: {ex.Message}");
            }

            return result;
        }
    }
}