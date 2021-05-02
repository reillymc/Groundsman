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
    public class FeatureDetailsViewModel : BaseFeatureDetailsViewModel
    {
        public ICommand GetFeatureCommand { get; set; }
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
        public FeatureDetailsViewModel(GeoJSONType geometryType)
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
        public FeatureDetailsViewModel(Feature feature)
        {
            Title = NameEntry = (string)feature.Properties[Constants.NameProperty];
            DateEntry = (string)feature.Properties[Constants.DateProperty];

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
            GetFeatureCommand = new Command<DisplayPosition>(async (point) => { await GetDataPoint(point); }, (point) => { return !IsBusy; });
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
                    File = new ShareFile(await FeatureStore.ExportFeature(Feature), "application/json")
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
                await Application.Current.MainPage.DisplayAlert("Unable To Fetch Location", "Ensure Groundsman has access to your device's location.", "Ok");
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
                bool saveSuccess = (string)Feature.Properties[Constants.IdentifierProperty] == Constants.NewFeatureID ? await FeatureStore.AddItem(Feature) : await FeatureStore.UpdateItem(Feature);
                if (saveSuccess)
                {
                    await NavigationService.NavigateBack(true);
                }
                else
                {
                    await NavigationService.ShowAlert("Save Failed", "Please check all of your entried are valid", false);
                }
                IsBusy = false;
                return;
            }
            IsBusy = false;
        }

        public override async Task DiscardDismiss()
        {
            await OnDismiss(true);
        }

        private async Task<bool> ValidateGeometry()
        {
            try
            {
                switch (GeometryType)
                {
                    case GeoJSONType.Point:
                        Feature.Geometry = new Point(new Position(Positions[0]));
                        break;
                    case GeoJSONType.LineString:
                        Feature.Geometry = new LineString(Positions.Select(pointValue => new Position(pointValue)).ToList());
                        break;
                    case GeoJSONType.Polygon:
                        // This method does not allow for creating a polygon with multiple LinearRings
                        List<Position> positions = Positions.Select(pointValue => new Position(pointValue)).ToList();
                        // Close polygon with duplicated first feature
                        positions.Add(positions[0]);
                        Feature.Geometry = new Polygon(new List<LinearRing>() { new LinearRing(positions) });
                        break;
                    default:
                        throw new ArgumentException($"Could not save unsupported feature of type {Feature.Geometry.Type}", Feature.Geometry.Type.ToString());
                }
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
            Dictionary<string, object> FinalProperties = new Dictionary<string, object>(HiddenProperties);
            //IEnumerable<Property> OrderedFeatureProperties = FeatureProperties.OrderBy(property => property.Key); can allow for ordering later
            foreach (Property property in Properties)
            {
                if (!string.IsNullOrEmpty(property.Key.ToString()))
                {
                    try
                    {
                        switch (property.Type)
                        {
                            case 0:
                                FinalProperties[property.Key] = property.Value;
                                break;
                            case 1:
                                int intValue = Convert.ToInt16(property.Value);
                                FinalProperties[property.Key] = intValue;
                                break;
                            case 2:
                                float floatValue = Convert.ToSingle(property.Value);
                                FinalProperties[property.Key] = floatValue;
                                break;
                            case 3:
                                bool boolValue = Convert.ToBoolean(property.Value);
                                FinalProperties[property.Key] = boolValue;
                                break;
                            default:
                                break;
                        }
                    }

                    catch
                    {
                        await NavigationService.ShowAlert("Invalid Feature Property", $"{property.Key} '{property.Value}' is incorrectly formatted.", false);
                        return false;
                    }

                }
                else
                {
                    Feature.Properties.Remove(property.Key);
                }
            }

            FinalProperties[Constants.NameProperty] = !string.IsNullOrEmpty(NameEntry) ? NameEntry : Feature.Geometry.Type.ToString();
            FinalProperties[Constants.DateProperty] = DateTime.Parse(DateEntry).ToShortDateString();
            FinalProperties[Constants.AuthorProperty] = Preferences.Get(Constants.UserIDKey, Constants.DefaultUserValue);

            Feature.Properties = FinalProperties;
            return true;
        }
    }
}
