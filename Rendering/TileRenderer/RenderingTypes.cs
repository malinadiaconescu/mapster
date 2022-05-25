using Mapster.Common.MemoryMappedTypes;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Mapster.Rendering;

public struct GeoFeature : BaseShape
{
    public enum GeoFeatureType
    {
        Plain,
        Hills,
        Mountains,
        Forest,
        Desert,
        Unknown,
        Water,
        Residential
    }

    public int ZIndex
    {
        get
        {
            switch (Type)
            {
                case GeoFeatureType.Plain:
                    return 10;
                case GeoFeatureType.Hills:
                    return 12;
                case GeoFeatureType.Mountains:
                    return 13;
                case GeoFeatureType.Forest:
                    return 11;
                case GeoFeatureType.Desert:
                    return 9;
                case GeoFeatureType.Unknown:
                    return 8;
                case GeoFeatureType.Water:
                    return 40;
                case GeoFeatureType.Residential:
                    return 41;
            }

            return 7;
        }
        set { }
    }

    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }
    public GeoFeatureType Type { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var color = Color.Magenta;
        switch (Type)
        {
            case GeoFeatureType.Plain:
                color = Color.LightGreen;
                break;
            case GeoFeatureType.Hills:
                color = Color.DarkGreen;
                break;
            case GeoFeatureType.Mountains:
                color = Color.LightGray;
                break;
            case GeoFeatureType.Forest:
                color = Color.Green;
                break;
            case GeoFeatureType.Desert:
                color = Color.SandyBrown;
                break;
            case GeoFeatureType.Unknown:
                color = Color.Magenta;
                break;
            case GeoFeatureType.Water:
                color = Color.LightBlue;
                break;
            case GeoFeatureType.Residential:
                color = Color.LightCoral;
                break;
        }

        if (!IsPolygon)
        {
            var pen = new Pen(color, 1.2f);
            context.DrawLines(pen, ScreenCoordinates);
        }
        else
        {
            context.FillPolygon(color, ScreenCoordinates);
        }
    }

    public GeoFeature(ReadOnlySpan<Coordinate> c, GeoFeatureType type)
    {
        IsPolygon = true;
        Type = type;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public GeoFeature(ReadOnlySpan<Coordinate> c, MapFeatureData feature)
    {
        IsPolygon = feature.Type == GeometryType.Polygon;
        var landauxiliar = feature.Properties.natural;
        Type = GeoFeatureType.Unknown;
        //change from comparing strings to comparing the enums mapped
        if (landauxiliar != PropertiesSettings.Land.NONE)
        {
            if (landauxiliar == PropertiesSettings.Land.FELL ||
                landauxiliar == PropertiesSettings.Land.GRASSLAND ||
                landauxiliar == PropertiesSettings.Land.HEATH ||
                landauxiliar == PropertiesSettings.Land.MOOR ||
                landauxiliar == PropertiesSettings.Land.SCRUB ||
                landauxiliar == PropertiesSettings.Land.WETLAND)
            {
                Type = GeoFeatureType.Plain;
            }
            else if (landauxiliar == PropertiesSettings.Land.WOOD || 
                     landauxiliar == PropertiesSettings.Land.TREE_ROW)
            {
                Type = GeoFeatureType.Forest;
            }
            else if (landauxiliar == PropertiesSettings.Land.BARE_ROCK || 
                     landauxiliar == PropertiesSettings.Land.ROCK ||
                     landauxiliar == PropertiesSettings.Land.SCREE)
            {
                Type = GeoFeatureType.Mountains;
            }
            else if (landauxiliar == PropertiesSettings.Land.BEACH ||
                     landauxiliar == PropertiesSettings.Land.SAND)
            {
                Type = GeoFeatureType.Desert;
            }
            else if (landauxiliar == PropertiesSettings.Land.WATER)
            {
                Type = GeoFeatureType.Water;
            }
        }

        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool isNatural(MapFeatureData feature) {
        return feature.Type == GeometryType.Polygon && feature.Properties.natural != PropertiesSettings.Land.NONE;  
    }

    public static bool isForest(MapFeatureData feature) {
        return feature.Properties.boundary == PropertiesSettings.Boundary.FOREST;
    }

    public static bool isLanduseForestOrOrchad(MapFeatureData feature) {
        return feature.Properties.landuse == PropertiesSettings.Landuse.FOREST || feature.Properties.landuse == PropertiesSettings.Landuse.ORCHARD;
    }

    public static bool isLanduseResidential(MapFeatureData feature) {
        PropertiesSettings.Landuse landuse = feature.Properties.landuse;
        return landuse == PropertiesSettings.Landuse.RESIDENTIAL || landuse == PropertiesSettings.Landuse.CEMETERY || landuse == PropertiesSettings.Landuse.INDUSTRIAL ||
          landuse == PropertiesSettings.Landuse.COMMERCIAL ||  landuse == PropertiesSettings.Landuse.SQUARE || landuse == PropertiesSettings.Landuse.CONSTRUCTION ||
          landuse == PropertiesSettings.Landuse.MILITARY || landuse == PropertiesSettings.Landuse.QUARRY || landuse == PropertiesSettings.Landuse.BROWNFIELD;
    }

    public static bool isLandusePlain(MapFeatureData feature) {
        PropertiesSettings.Landuse landuse = feature.Properties.landuse;
        return landuse == PropertiesSettings.Landuse.FARM || landuse == PropertiesSettings.Landuse.MEADOW || landuse == PropertiesSettings.Landuse.GRASS ||
          landuse == PropertiesSettings.Landuse.GREENFIELD || landuse == PropertiesSettings.Landuse.RECREATION_GROUND || landuse == PropertiesSettings.Landuse.WINTER_SPORTS ||
          landuse == PropertiesSettings.Landuse.ALLOTMENTS;
    }

    public static bool isWater(MapFeatureData feature) {
        PropertiesSettings.Landuse landuse = feature.Properties.landuse;
        return landuse == PropertiesSettings.Landuse.RESERVOIR || landuse == PropertiesSettings.Landuse.BASIN;
    }

    public static bool isBuilding(MapFeatureData feature) {
      return feature.Properties.building != PropertiesSettings.Building.NONE && feature.Type == GeometryType.Polygon;
    }

    public static bool isLeisure(MapFeatureData feature) {
      return feature.Properties.leisure != PropertiesSettings.Leisure.NONE && feature.Type == GeometryType.Polygon;
    }

    public static bool isAmenity(MapFeatureData feature) {
      return feature.Properties.amenity != PropertiesSettings.Amenity.NONE && feature.Type == GeometryType.Polygon;
    }
}

public struct Railway : BaseShape
{
    public int ZIndex { get; set; } = 45;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var penA = new Pen(Color.DarkGray, 2.0f);
        var penB = new Pen(Color.LightGray, 1.2f, new[]
        {
            2.0f, 4.0f, 2.0f
        });
        context.DrawLines(penA, ScreenCoordinates);
        context.DrawLines(penB, ScreenCoordinates);
    }

    public Railway(ReadOnlySpan<Coordinate> c)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool isRailway(MapFeatureData feature) {
      return feature.Properties.railway != PropertiesSettings.Railway.NONE;
    }
}

public struct PopulatedPlace : BaseShape
{
    public int ZIndex { get; set; } = 60;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }
    public string Name { get; set; }
    public bool ShouldRender { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!ShouldRender)
        {
            return;
        }
        var font = SystemFonts.Families.First().CreateFont(12, FontStyle.Bold);
        context.DrawText(Name, font, Color.Black, ScreenCoordinates[0]);
    }

    public PopulatedPlace(ReadOnlySpan<Coordinate> c, MapFeatureData feature)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
        var name = feature.Properties.name;

        if (feature.Label.IsEmpty)
        {
            ShouldRender = false;
            Name = "Unknown";
        }
        else
        {
            Name = string.IsNullOrWhiteSpace(name) ? feature.Label.ToString() : name;
            ShouldRender = true;
        }
    }

    public static bool isPopulatedPlace(MapFeatureData feature)
    {
        // https://wiki.openstreetmap.org/wiki/Key:place
        if (feature.Type != GeometryType.Point)
        {
            return false;
        }
        PropertiesSettings.Setting place = feature.Properties.place;
        return place == PropertiesSettings.Setting.CITY || place == PropertiesSettings.Setting.TOWN || place == PropertiesSettings.Setting.LOCALITY || place == PropertiesSettings.Setting.HAMLET;
    }
}

public struct Border : BaseShape
{
    public int ZIndex { get; set; } = 30;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var pen = new Pen(Color.Gray, 2.0f);
        context.DrawLines(pen, ScreenCoordinates);
    }

    public Border(ReadOnlySpan<Coordinate> c)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool isBorder(MapFeatureData feature)
    {
        return feature.Properties.boundary == PropertiesSettings.Boundary.ADMINISTRATIVE && feature.Properties.adminLevel == PropertiesSettings.AdministrationLevel.LEVEL_2;
    }
}

public struct Waterway : BaseShape
{
    public int ZIndex { get; set; } = 40;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!IsPolygon)
        {
            var pen = new Pen(Color.LightBlue, 1.2f);
            context.DrawLines(pen, ScreenCoordinates);
        }
        else
        {
            context.FillPolygon(Color.LightBlue, ScreenCoordinates);
        }
    }
    
    public static bool isWaterway(MapFeatureData feature)
    {
      return feature.Properties.water != PropertiesSettings.Water.NONE && feature.Type != GeometryType.Point;
    }

    public Waterway(ReadOnlySpan<Coordinate> c, bool isPolygon = false)
    {
        IsPolygon = isPolygon;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }
}

public struct Road : BaseShape
{
    public int ZIndex { get; set; } = 50;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!IsPolygon)
        {
            var pen = new Pen(Color.Coral, 2.0f);
            var pen2 = new Pen(Color.Yellow, 2.2f);
            context.DrawLines(pen2, ScreenCoordinates);
            context.DrawLines(pen, ScreenCoordinates);
        }
    }

    public Road(ReadOnlySpan<Coordinate> c, bool isPolygon = false)
    {
        IsPolygon = isPolygon;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool isRoad(MapFeatureData feature)
    {
        return feature.Properties.highway != PropertiesSettings.Highway.SOMETHING && feature.Properties.highway != PropertiesSettings.Highway.NONE;
    }
    
}

public interface BaseShape
{
    public int ZIndex { get; set; }
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context);

    public void TranslateAndScale(float minX, float minY, float scale, float height)
    {
        for (var i = 0; i < ScreenCoordinates.Length; i++)
        {
            var coord = ScreenCoordinates[i];
            var newCoord = new PointF((coord.X + minX * -1) * scale, height - (coord.Y + minY * -1) * scale);
            ScreenCoordinates[i] = newCoord;
        }
    }
}
