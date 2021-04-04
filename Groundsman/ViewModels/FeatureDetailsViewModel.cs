using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Point = Groundsman.Models.Point;
using Position = Groundsman.Models.Position;
using System.Linq;

namespace Groundsman.ViewModels
{
    /// <summary>
    /// View-model for the page that shows a data entry's details in a form.
    /// </summary>
    public class FeatureDetailsViewModel : BaseViewModel
    {
        public ICommand GetFeatureCommand { get; set; }
        public ICommand AddPointCommand { get; set; }
        public ICommand DeletePointCommand { get; set; }
        public ICommand OnDoneTappedCommand { get; set; }
        public ICommand OnCancelTappedCommand { get; set; }
        public ICommand ShareEntryCommand { get; set; }
        public ICommand AddPropertyCommand { get; set; }
        public ICommand DeletePropertyCommand { get; set; }

        private readonly Feature Feature = new Feature { };

        public List<string> PropertyTypes { get; set; } = new List<string>() { "String", "Integer", "Float", "Boolean" };

        public ObservableCollection<DisplayPosition> GeolocationValues { get; set; }
        public ObservableCollection<Property> FeatureProperties { get; set; }

        public string DateEntry { get; set; }
        public string NameEntry { get; set; }
        public string MetadataStringEntry { get; set; } = "";
        public string MetadataIntegerEntry { get; set; } = "";
        public string MetadataFloatEntry { get; set; } = "";

        public bool ShowAddButton { get; set; }
        public bool ShowClosePolygon { get; set; }

        private bool loadingIconActive;
        public bool LoadingIconActive
        {
            get { return loadingIconActive; }
            set { SetProperty(ref loadingIconActive, value); }
        }

        private bool geolocationEntryEnabled = true;
        public bool GeolocationEntryEnabled
        {
            get { return geolocationEntryEnabled; }
            set { SetProperty(ref geolocationEntryEnabled, value); }
        }

        private int _NumPointFields;
        public int NumPointFields
        {
            get { return _NumPointFields; }
            set
            {
                _NumPointFields = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// View-model constructor for adding new entries.
        /// </summary>
        public FeatureDetailsViewModel(GeoJSONType featureType)
        {
            //this.featureType = featureType;
            Feature.Geometry = new Geometry(featureType);
            Feature.Properties = new Dictionary<string, object>
            {
                ["id"] = AppConstants.NEW_ENTRY_ID
            };

            DateEntry = DateTime.Now.ToShortDateString();
            GeolocationValues = new ObservableCollection<DisplayPosition>();

            InitCommandBindings();

            switch (featureType)
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
            }
            NumPointFields = GeolocationValues.Count + 1;

            FeatureProperties = new ObservableCollection<Property>
            {
                new Property("Default String Property", string.Empty, 0),
                new Property("Default Integer Property", null, 1),
                new Property("Default Float Property", null, 2)
            };
        }

        /// <summary>
        /// View-model constructor for viewing/editing existing entries.
        /// </summary>
        public FeatureDetailsViewModel(Feature feature)
        {
            InitCommandBindings();
            Feature = feature;

            if (feature.Properties.ContainsKey("name"))
            {
                Title = NameEntry = (string)feature.Properties["name"];
            }

            if (feature.Properties.ContainsKey("date"))
            {
                DateEntry = (string)feature.Properties["date"];
            }


            GeolocationValues = new ObservableCollection<DisplayPosition>();
            
            int index = 1;

            //SWITCH TEMPLATE - maybe make method?
            switch (feature.Geometry.Type)
            {
                case GeoJSONType.Point:
                    Point point = (Point)feature.Geometry;
                    GeolocationValues.Add(new DisplayPosition("1", point.Coordinates));
                    break;
                case GeoJSONType.LineString:
                    LineString linestring = (LineString)feature.Geometry;
                    foreach (Position pos in linestring.Coordinates)
                    {
                        GeolocationValues.Add(new DisplayPosition(index.ToString(), pos));
                        index++;
                    }
                    ShowAddButton = true;
                    break;
                case GeoJSONType.Polygon:
                    Polygon polygon = (Polygon)feature.Geometry;
                    foreach (LineString ls in polygon.Coordinates)
                    {
                        foreach (Position pos in ls.Coordinates)
                        {
                            GeolocationValues.Add(new DisplayPosition(index.ToString(), pos));
                            index++;
                        }
                    }
                    //Remove last position so that poly can be closed duplicating the first posiiton back to the end after editing
                    if (GeolocationValues[0].Equals(GeolocationValues[^1]))
                    {
                        GeolocationValues.RemoveAt(GeolocationValues.Count - 1);
                    }
                    ShowAddButton = true;
                    ShowClosePolygon = true;
                    break;
                default:
                    //Unrecognised feature alert!!
                    break;

            }
            NumPointFields = GeolocationValues.Count + 1;

            FeatureProperties = new ObservableCollection<Property>();

            foreach (KeyValuePair<string, object> property in feature.Properties)
            {
                if (property.Key != "author" && property.Key != "name" && property.Key != "id" && property.Key != "date")
                {
                    FeatureProperties.Add(Property.FromObject(property.Key.ToString(), property.Value));
                }
            }
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<DisplayPosition>(async (point) => { await GetDataPoint(point); });
            AddPointCommand = new Command(() => AddPoint(1));
            DeletePointCommand = new Command<DisplayPosition>((item) => DeletePoint(item));
            AddPropertyCommand = new Command(() => AddProperty());
            DeletePropertyCommand = new Command<Property>((item) => DeleteProperty(item));
            ShareEntryCommand = new Command<View>(async (view) => await ShareFeature(view));
            OnDoneTappedCommand = new Command(async () => await OnSaveUpdateActivated());
            OnCancelTappedCommand = new Command(async () => await OnDismiss(true));
        }

        private void DeleteProperty(Property item)
        {
            FeatureProperties.Remove(item);
        }

        private void AddProperty()
        {
            //FeatureProperties.Insert(0, new Property("", ""));
            FeatureProperties.Add(new Property("", ""));
        }


        private async Task ShareFeature(View element)
        {
            if (IsBusy) return;
            IsBusy = true;

            if (await ValidateGeometry() && await ValidateProperties())
            {
                var bounds = element.GetAbsoluteBounds().ToSystemRectangle();
                ShareFileRequest share = FeatureStore.ExportFeatures(new List<Feature>() { Feature });
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
            GeolocationEntryEnabled = false;
            LoadingIconActive = true;

            Position location = await HelperServices.GetGeoLocation();
            DisplayPosition convertedPoint = new DisplayPosition("0", location);
            if (location != null)
            {
                point.Latitude = convertedPoint.Latitude;
                point.Longitude = convertedPoint.Longitude;
                point.Altitude = convertedPoint.Altitude;
            }
            GeolocationEntryEnabled = true;
            LoadingIconActive = false;
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
                GeolocationValues.Add(new DisplayPosition((GeolocationValues.Count + 1).ToString(), "", "", ""));
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
            if (GeolocationValues.Count == 1)
            {
                NavigationService.ShowAlert("Cannot Remove Position", "All features must have at least one position", false);
                return;
            }
            IsBusy = true;
            // Workaround for when removing GeolocationValues[0] not updating front-end close polygon fields referencing GeolocationValues[0]
            if (item == GeolocationValues[0])
            {
                GeolocationValues[0].Longitude = GeolocationValues[1].Longitude;
                GeolocationValues[0].Latitude = GeolocationValues[1].Latitude;
                GeolocationValues[0].Altitude = GeolocationValues[1].Altitude;
                GeolocationValues.RemoveAt(1);
            }
            else
            {
                GeolocationValues.Remove(item);
            }
            for (int i = 0; i < GeolocationValues.Count; i++)
            {
                GeolocationValues[i].Index = (i + 1).ToString();
            }
            NumPointFields--;
            IsBusy = false;
        }

        /// <summary>
        /// Saves a new or edited feature to the embedded file.
        /// </summary>
        async Task OnSaveUpdateActivated()
        {
            if (IsBusy) return;
            IsBusy = true;
            if (await ValidateGeometry() && await ValidateProperties())
            {
                if (await SaveFeature())
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

        private async Task<bool> ValidateGeometry()
        {
            try
            {
                switch (Feature.Geometry.Type)
                {
                    case GeoJSONType.Point:
                        Feature.Geometry = new Point(new Position(GeolocationValues[0]));
                        break;
                    case GeoJSONType.LineString:
                        Feature.Geometry = new LineString(GeolocationValues.Select(pointValue => new Position(pointValue)));
                        break;
                    case GeoJSONType.Polygon:
                        // This method does not allow for creating a polygon with multiple LinearRings
                        List<Position> positions = (GeolocationValues.Select(pointValue => new Position(pointValue))).ToList();
                        // Close polygon with duplicated first feature
                        positions.Add(positions[0]);
                        Feature.Geometry = new Polygon(new List<LinearRing>() { new LinearRing(positions) });
                        break;
                    default:
                        throw new ArgumentException($"Could not save unsupported feature of type {Feature.Geometry.Type}", "Type");
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
            string IDTemp = (string)Feature.Properties["id"];
            Feature.Properties.Clear();
            Feature.Properties.Add("id", IDTemp);
            //IEnumerable<Property> OrderedFeatureProperties = FeatureProperties.OrderBy(property => property.Key); can allow for fordering later
            foreach (Property property in FeatureProperties)
            {
                if (!string.IsNullOrEmpty(property.Key.ToString()))
                {
                    try
                    {
                        switch (property.Type)
                        {
                            case 0:
                                Feature.Properties[property.Key] = property.Value;
                                break;
                            case 1:
                                int intValue = Convert.ToInt16(property.Value);
                                Feature.Properties[property.Key] = intValue;
                                break;
                            case 2:
                                float floatValue = Convert.ToSingle(property.Value);
                                Feature.Properties[property.Key] = floatValue;
                                break;
                            case 3:
                                bool boolValue = Convert.ToBoolean(property.Value);
                                Feature.Properties[property.Key] = boolValue;
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

            Feature.Properties["name"] = !string.IsNullOrEmpty(NameEntry) ? NameEntry : Feature.Geometry.Type.ToString();
            Feature.Properties["date"] = DateTime.Parse(DateEntry).ToShortDateString();
            Feature.Properties["author"] = Preferences.Get("UserID", "Groundsman");

            return true;
        }

        private async Task<bool> SaveFeature()
        {
            return (string)Feature.Properties["id"] == AppConstants.NEW_ENTRY_ID ? await FeatureStore.AddItemAsync(Feature) : await FeatureStore.UpdateItemAsync(Feature);
        }
    }
}
