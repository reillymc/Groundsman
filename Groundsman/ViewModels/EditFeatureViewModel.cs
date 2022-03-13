using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Point = Groundsman.Models.Point;
using Position = Groundsman.Models.Position;

namespace Groundsman.ViewModels;

/// <summary>
/// View-model for the page that shows a data entry's details in a form.
/// </summary>
public class EditFeatureViewModel : BaseEditFeatureViewModel
{
    public ICommand AddPointCommand { get; set; }
    public ICommand DeletePointCommand { get; set; }

    /// <summary>
    /// ViewModel constructor for creating a new feature.
    /// </summary>
    public EditFeatureViewModel(GeoJSONType geometryType)
    {
        Title = $"New {geometryType}";
        DateEntry = DateTime.Now;

        Id = Guid.NewGuid().ToString();
        GeometryType = geometryType;

        AddDefaultProperties();


        switch (geometryType)
        {
            case GeoJSONType.Point:
                AddPosition(1);
                break;
            case GeoJSONType.LineString:
                AddPosition(2);
                break;
            case GeoJSONType.Polygon:
                AddPosition(3);
                break;
            default:
                throw new ArgumentException("Feature type not supported", geometryType.ToString());
        }

        InitCommandBindings();
    }

    /// <summary>
    /// ViewModel constructor for editing an exisiting feature.
    /// </summary>
    public EditFeatureViewModel(Feature feature)
    {
        InitialiseExisitingFeature(feature);

        int index = 1;
        switch (feature.Geometry.Type)
        {
            case GeoJSONType.Point:
                Point point = (Point)feature.Geometry;
                Positions.Add(new DisplayPosition(index.ToString(), point.Coordinates));
                break;
            case GeoJSONType.LineString:
                LineString lineString = (LineString)feature.Geometry;
                foreach (Position pos in lineString.Coordinates)
                {
                    Positions.Add(new DisplayPosition(index.ToString(), pos));
                    index++;
                }
                break;
            case GeoJSONType.Polygon:
                Polygon polygon = (Polygon)feature.Geometry;
                foreach (LineString ls in polygon.Coordinates)
                {
                    // TODO: Display warning if more than one LR
                    foreach (Position pos in ls.Coordinates)
                    {
                        Positions.Add(new DisplayPosition(index.ToString(), pos));
                        index++;
                    }
                }
                //Remove last position so that poly can be closed duplicating the first posiiton back to the end after editing
                if (Positions[0].Equals(Positions[^1]))
                {
                    Positions.RemoveAt(Positions.Count - 1);
                }
                break;
            default:
                throw new ArgumentException("Feature type not supported", feature.Type.ToString());

        }

        InitCommandBindings();
    }

    /// <summary>
    /// Initialise page-specific command bindings.
    /// </summary>
    private void InitCommandBindings()
    {
        GetFeatureCommand = new Command<DisplayPosition>(async (point) => { await SetPositionToCurrentLocation(point); });
        AddPointCommand = new Command(() => AddPosition(1));
        DeletePointCommand = new Command<DisplayPosition>((item) => DeletePosition(item));
        ShareButtonClickCommand = new Command<View>(async (view) => await ShareFeature(view));
    }

    public override async Task SaveDismiss()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            Feature saveFeature = GetValidatedFeature();
            int saveSuccess = await FeatureStore.SaveItem(saveFeature);
            if (saveSuccess > 0)
            {
                _ = await FeatureStore.GetItemsAsync();
                await NavigationService.NavigateBack(true);
            }
            else
            {
                await NavigationService.ShowAlert("Save Failed", $"Please check all of your entries are valid", false);
            }
        }
        catch (Exception ex)
        {
            await NavigationService.ShowAlert("Invalid Feature Data", $"{ex.Message}", false);

        }
        IsBusy = false;
    }

    public override async Task CancelDismiss()
    {
        await OnDismiss(true);
    }

    public async Task ShareFeature(View element)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            Feature saveFeature = GetValidatedFeature();
            await ShareFeature(saveFeature, element);
        }
        catch (Exception ex)
        {
            await NavigationService.ShowAlert("Invalid Feature Data", $"{ex.Message}", false);

        }
        IsBusy = false;
    }

    /// <summary>
    /// Queries the current device's location coordinates and applies them to the given point
    /// </summary>
    /// <param name="point">Point to set GPS data to.</param>
    private async Task SetPositionToCurrentLocation(DisplayPosition point)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var index = Positions.IndexOf(point);
            Position location = await HelperServices.GetGeoLocation();
            Positions[index] = new DisplayPosition((index + 1).ToString(), location);
        }
        catch
        {
            await NavigationService.ShowAlert("Unable To Fetch Location", "Ensure Groundsman has access to your device's location.", false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Adds a new position.
    /// </summary>
    /// <returns></returns>
    private void AddPosition(int count)
    {
        if (IsBusy) return;
        IsBusy = true;
        for (int i = 0; i < count; i++)
        {
            Positions.Add(new DisplayPosition((Positions.Count + 1).ToString(), "", "", ""));
        }
        IsBusy = false;
    }

    /// <summary>
    /// Deletes a position.
    /// </summary>
    /// <param name="item">Item to delete</param>
    private void DeletePosition(DisplayPosition item)
    {
        if (IsBusy) return;
        if (Positions.Count == 1)
        {
            NavigationService.ShowAlert("Cannot Remove Position", "All features must have at least one position", false);
            return;
        }
        IsBusy = true;
        // Workaround for when removing GeolocationValues[0] not updating front-end close polygon fields referencing GeolocationValues[0]
        if (item == Positions[0])
        {
            Positions[0].Longitude = Positions[1].Longitude;
            Positions[0].Latitude = Positions[1].Latitude;
            Positions[0].Altitude = Positions[1].Altitude;
            Positions.RemoveAt(1);
        }
        else
        {
            Positions.Remove(item);
        }
        for (int i = 0; i < Positions.Count; i++)
        {
            Positions[i].Index = (i + 1).ToString();
        }
        IsBusy = false;
    }
}
