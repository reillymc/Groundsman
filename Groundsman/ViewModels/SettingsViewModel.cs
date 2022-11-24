using Microsoft.Toolkit.Mvvm.Input;
using Groundsman.Helpers;
using Groundsman.Services;
using Groundsman.Pages;

namespace Groundsman.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private FeatureService featureService;

    public string UserName
    {
        get => Settings.UserName;
        set => Settings.UserName = !string.IsNullOrWhiteSpace(value) ? value : "Groundsman";
    }

    public int DecimalPrecision
    {
        get => Settings.DecimalPrecision;
        set => Settings.DecimalPrecision = value;
    }

    public int GeolocationAccuracy
    {
        get => Settings.GeolocationAccuracy;
        set => Settings.GeolocationAccuracy = value;
    }

    public bool ShakeToUndo
    {
        get => Settings.ShakeToUndo;
        set => Settings.ShakeToUndo = value;
    }

    public bool EditorMapPreview
    {
        get => Settings.EditorMapPreview;
        set => Settings.EditorMapPreview = value;
    }

    public int LoggerExportFormat
    {
        get => Settings.LoggerExportFormat;
        set => Settings.LoggerExportFormat = value;
    }

    public bool MapRenderPoints
    {
        get => Settings.MapRenderPoints;
        set => Settings.MapRenderPoints = value;
    }
    public bool MapRenderLines
    {
        get => Settings.MapRenderLines;
        set => Settings.MapRenderLines = value;
    }
    public bool MapRenderPolygons
    {
        get => Settings.MapRenderPolygons;
        set => Settings.MapRenderPolygons = value;
    }

    public SettingsViewModel(FeatureService featureService)
    {
        this.featureService = featureService;
    }

    [ICommand]
    private async Task ViewInfo()
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }

    [ICommand]
    private async Task DeleteAllFeatures()
    {
        var confirmation = await Shell.Current.DisplayAlert("Reset Feature List?", "This will permanently erase all saved features.", "Reset", "Cancel");
        if (confirmation)
        {
            _ = featureService.ClearItems();
        }
        return;
    }
}

