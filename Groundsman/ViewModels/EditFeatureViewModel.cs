using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Point = Groundsman.Models.Point;
using Position = Groundsman.Models.Position;

namespace Groundsman.ViewModels
{
    /// <summary>
    /// View-model for the page that shows a data entry's details in a form.
    /// </summary>
    public class EditFeatureViewModel : BaseEditFeatureViewModel
    {
        public ICommand AddPointCommand { get; set; }
        public ICommand DeletePointCommand { get; set; }
        public ICommand AddPropertyCommand { get; set; }
        public ICommand DeletePropertyCommand { get; set; }

        private readonly GeoJSONType GeometryType;
        private readonly Dictionary<string, object> HiddenProperties = new Dictionary<string, object>();

        public ObservableCollection<Property> Properties { get; set; } = new ObservableCollection<Property>();

        public bool ShowAddButton { get; set; }
        public bool ShowClosePolygon { get; set; }

        public bool isLogLine { get; set; } = false;
        public bool isRegularFeature { get; set; } = true;

        private int _NumPointFields;
        public int NumPointFields
        {
            get => _NumPointFields;
            set
            {
                _NumPointFields = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// View-model constructor for adding new entries.
        /// </summary>
        public EditFeatureViewModel(GeoJSONType geometryType)
        {
            GeometryType = geometryType;

            HiddenProperties.Add(Constants.IdentifierProperty, Constants.NewFeatureID);

            Properties.Add(new Property("String Property", string.Empty, 0));
            Properties.Add(new Property("Integer Property", null, 1));
            Properties.Add(new Property("Float Property", null, 2));

            DateEntry = DateTime.Now.ToShortDateString();

            switch (geometryType)
            {
                case GeoJSONType.Point:
                    Title = "New Point";
                    AddPoint(1);
                    break;
                case GeoJSONType.LineString:
                    Title = "New Line";
                    ShowAddButton = true;
                    AddPoint(2);
                    break;
                case GeoJSONType.Polygon:
                    Title = "New Polygon";
                    ShowAddButton = true;
                    ShowClosePolygon = true;
                    AddPoint(3);
                    break;
                default:
                    throw new ArgumentException("Feature type not supported", geometryType.ToString());
            }
            NumPointFields = Positions.Count + 1;

            InitCommandBindings();
        }

        /// <summary>
        /// View-model constructor for viewing/editing existing entries.
        /// </summary>
        public EditFeatureViewModel(Feature feature)
        {
            Title = NameEntry = feature.Name;
            DateEntry = (string)feature.Date;
            Feature.Id = feature.Id;

            GeometryType = feature.Geometry.Type;

            int index = 1;
            switch (feature.Geometry.Type)
            {
                case GeoJSONType.Point:
                    Point point = (Point)feature.Geometry;
                    Positions.Add(new DisplayPosition("1", point.Coordinates));
                    break;
                case GeoJSONType.LineString:
                    LineString linestring = (LineString)feature.Geometry;
                    foreach (Position pos in linestring.Coordinates)
                    {
                        Positions.Add(new DisplayPosition(index.ToString(), pos));
                        index++;
                    }
                    ShowAddButton = true;
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
                    ShowAddButton = true;
                    ShowClosePolygon = true;
                    break;
                default:
                    throw new ArgumentException("Feature type not supported", feature.Type.ToString());

            }
            NumPointFields = Positions.Count + 1;

            foreach (KeyValuePair<string, object> property in feature.Properties)
            {
                if (property.Key != Constants.AuthorProperty && property.Key != Constants.NameProperty && property.Key != Constants.IdentifierProperty && property.Key != Constants.DateProperty && property.Key != Constants.LogTimestampsProperty)
                {
                    Properties.Add(Property.FromObject(property.Key.ToString(), property.Value));
                }
                else
                {
                    HiddenProperties.Add(property.Key, property.Value);
                }
            }

            InitCommandBindings();
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<DisplayPosition>(async (point) => { await GetDataPoint(point); });
            AddPointCommand = new Command(() => AddPoint(1));
            DeletePointCommand = new Command<DisplayPosition>((item) => DeletePoint(item));
            AddPropertyCommand = new Command(() => Properties.Add(new Property("", "")));
            DeletePropertyCommand = new Command<Property>((item) => Properties.Remove(item));

        }

        public override async Task ShareFeature(View element)
        {
            if (IsBusy) return;
            IsBusy = true;

            if (await ValidateGeometry() && await ValidateProperties())
            {
                System.Drawing.Rectangle bounds = element.GetAbsoluteBounds().ToSystemRectangle();
                ShareFileRequest share = new ShareFileRequest
                {
                    Title = "Share Feature",
                    File = new ShareFile(await FeatureHelper.ExportFeatures(Feature), "application/json")
                };
                share.PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet ? bounds : System.Drawing.Rectangle.Empty;
                await Share.RequestAsync(share);
            }
            IsBusy = false;
        }

        /// <summary>
        /// Queries the current device's location coordinates
        /// </summary>
        /// <param name="point">Point to set GPS data to.</param>
        private async Task GetDataPoint(DisplayPosition point)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Position location = await HelperServices.GetGeoLocation();
                DisplayPosition convertedPoint = new DisplayPosition("0", location);
                point.Latitude = convertedPoint.Latitude;
                point.Longitude = convertedPoint.Longitude;
                point.Altitude = convertedPoint.Altitude;
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
        /// Adds a new geolocation point to the list for line or polygon data types.
        /// </summary>
        /// <returns></returns>
        private void AddPoint(int count)
        {
            if (IsBusy) return;
            IsBusy = true;
            for (int i = 0; i < count; i++)
            {
                Positions.Add(new DisplayPosition((Positions.Count + 1).ToString(), "", "", ""));
                NumPointFields++;
            }
            IsBusy = false;
        }

        /// <summary>
        /// Deletes a geolocation point from the list.
        /// </summary>
        /// <param name="item">Item to delete</param>
        private void DeletePoint(DisplayPosition item)
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
            NumPointFields--;
            IsBusy = false;
        }

        /// <summary>
        /// Saves a new or edited feature to the embedded file.
        /// </summary>
        public override async Task SaveDismiss()
        {
            if (IsBusy) return;
            IsBusy = true;

            if (await ValidateGeometry() && await ValidateProperties())
            {
                Feature.Name = !string.IsNullOrEmpty(NameEntry) ? NameEntry : Feature.Geometry.Type.ToString();
                Feature.Date = DateTime.Parse(DateEntry).ToShortDateString();
                Feature.Author = Preferences.Get(Constants.UserIDKey, Constants.DefaultUserValue);

                if (Feature.Id == null)
                {
                    Feature.Id = Guid.NewGuid().ToString();
                }
                int saveSuccess = await FeatureStore.SaveItem(Feature);
                if (saveSuccess > 0)
                {
                    _ = await FeatureStore.GetItemsAsync();
                    await NavigationService.NavigateBack(true);
                }
                else
                {
                    await NavigationService.ShowAlert("Save Failed", "Please check all of your entries are valid", false);
                }
                IsBusy = false;
                return;
            }
            IsBusy = false;
        }

        public override async Task CancelDismiss()
        {
            await OnDismiss(true);
        }

        public override void AnyDismiss()
        {
            return;
        }

        private async Task<bool> ValidateGeometry()
        {
            try
            {
                Feature.Geometry = FeatureHelper.GetValidatedGeometry(Positions, GeometryType);
            }
            catch (Exception ex)
            {
                await NavigationService.ShowAlert("Invalid Feature Geometry", $"{ex.Message}", false);
                return false;
            }
            return true;
        }

        private async Task<bool> ValidateProperties()
        {
            try
            {
                Feature.Properties = FeatureHelper.GetValidatedProperties(Properties, HiddenProperties);
            }
            catch (Exception ex)
            {
                await NavigationService.ShowAlert("Invalid Feature Properties", $"{ex.Message}", false);
                return false;
            }
            return true;
        }
    }
}
