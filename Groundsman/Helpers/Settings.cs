namespace Groundsman.Helpers;

public static class Settings
{
    public static string UserName
    {
        get => Preferences.Get(nameof(UserName), Constants.DefaultUserId);
        set => Preferences.Set(nameof(UserName), value);
    }

    public static int GeolocationAccuracy
    {
        get => Preferences.Get(nameof(GeolocationAccuracy), 2);
        set => Preferences.Set(nameof(GeolocationAccuracy), value);
    }

    public static int DecimalPrecision
    {
        get => Preferences.Get(nameof(DecimalPrecision), 6);
        set => Preferences.Set(nameof(DecimalPrecision), value);
    }

    public static bool ShakeToUndo
    {
        get => Preferences.Get(nameof(ShakeToUndo), true);
        set => Preferences.Set(nameof(ShakeToUndo), value);
    }

    public static int LoggerExportFormat
    {
        get => Preferences.Get(nameof(LoggerExportFormat), 0);
        set => Preferences.Set(nameof(LoggerExportFormat), value);
    }

    public static bool EditorMapPreview
    {
        get => Preferences.Get(nameof(EditorMapPreview), true);
        set => Preferences.Set(nameof(EditorMapPreview), value);
    }

    public static bool MapRenderPoints
    {
        get => Preferences.Get(nameof(MapRenderPoints), true);
        set => Preferences.Set(nameof(MapRenderPoints), value);
    }

    public static bool MapRenderLines
    {
        get => Preferences.Get(nameof(MapRenderLines), true);
        set => Preferences.Set(nameof(MapRenderLines), value);
    }

    public static bool MapRenderPolygons
    {
        get => Preferences.Get(nameof(MapRenderPolygons), true);
        set => Preferences.Set(nameof(MapRenderPolygons), value);
    }
}