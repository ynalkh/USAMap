using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace MapSolution
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var states = StatesParser.Parse("states.json");
            double width = 1600;
            double height = 1000;
            MapImage.Source = DrawMap(states, width, height);
        }

        private void GetAllBounds(Dictionary<string, StateGeometry> states, 
            out double minLon, out double maxLon, out double minLat, out double maxLat)
        {
            minLon = double.MaxValue;
            maxLon = double.MinValue;
            minLat = double.MaxValue;
            maxLat = double.MinValue;

            foreach (var kvState in states)
            {
                foreach (var polygon in kvState.Value.Polygons)
                {
                    foreach (var p in polygon)
                    {
                        minLon = Math.Min(minLon, p.Lon);
                        maxLon = Math.Max(maxLon, p.Lon);
                        minLat = Math.Min(minLat, p.Lat);
                        maxLat = Math.Max(maxLat, p.Lat);
                    }
                }
            }
        }

        private Avalonia.Point ScreenPoint(double lon, double lat, 
            double minLon, double maxLon, double minLat, double maxLat, 
            double offsetX, double offsetY, double scale)
        {
            double x = offsetX + (lon - minLon) * scale;
            double y = offsetY + (maxLat - lat) * scale;
            return new Avalonia.Point(x, y);
        }

        private DrawingImage DrawMap(Dictionary<string, StateGeometry> states, double width, double height)
        {
            var group = new DrawingGroup();

            using (var context = group.Open())
            {
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

                var borderPen = new Pen(Brushes.Black, 1);
                var fillBrush = new SolidColorBrush(Color.FromRgb(173, 216, 230));
                
                GetAllBounds(states, out double minLon, out double maxLon, out double minLat, out double maxLat);

                double lonRange = maxLon - minLon;
                double latRange = maxLat - minLat;

                double padding = 50;
                double availableWidth = width - 2 * padding;
                double availableHeight = height - 2 * padding;

                double scale = Math.Min(availableWidth / lonRange, availableHeight / latRange);

                double mapWidth = lonRange * scale;
                double mapHeight = latRange * scale;

                double offsetX = padding + (availableWidth - mapWidth) / 2;
                double offsetY = padding + (availableHeight - mapHeight) / 2;

                foreach (var kvState in states)
                {
                    var state = kvState.Value;
                    
                    foreach (var polygon in state.Polygons)
                    {
                        if (polygon.Count < 3) continue;

                        var geometry = new StreamGeometry();
                        using (var geoContext = geometry.Open())
                        {
                            var firstPoint = ScreenPoint(polygon[0].Lon, polygon[0].Lat,
                                minLon, maxLon, minLat, maxLat, offsetX, offsetY, scale);
                            
                            geoContext.BeginFigure(firstPoint, true);
                            
                            for (int i = 1; i < polygon.Count; i++)
                            {
                                var point = ScreenPoint(polygon[i].Lon, polygon[i].Lat,
                                    minLon, maxLon, minLat, maxLat, offsetX, offsetY, scale);
                                geoContext.LineTo(point);
                            }
                            
                            geoContext.EndFigure(true);
                        }

                        context.DrawGeometry(fillBrush, borderPen, geometry);
                    }
                }
            }

            return new DrawingImage { Drawing = group };
        }
    }
}