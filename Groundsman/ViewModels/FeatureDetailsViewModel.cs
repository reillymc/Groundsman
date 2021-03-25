using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
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
    public class FeatureDetailsViewModel : BaseViewModel
    {
        public ICommand GetFeatureCommand { get; set; }
        public ICommand AddPointCommand { get; set; }
        public ICommand DeletePointCommand { get; set; }
        public ICommand OnDoneTappedCommand { get; set; }
        public ICommand OnCancelTappedCommand { get; set; }
        public ICommand ShareEntryCommand { get; set; }
        private Feature feature = new Feature { };
        private GeoJSONType featureType;
        private string FeatureID;
        public ObservableCollection<DisplayPosition> GeolocationValues { get; set; }

        public string DateEntry { get; set; }
        public string NameEntry { get; set; }
        public string MetadataStringEntry { get; set; }
        public string MetadataIntegerEntry { get; set; }
        public string MetadataFloatEntry { get; set; }

        public bool ShowAddButton { get; set; }
        public bool ShowClosePolygon { get; set; }

        private bool loadingIconActive;
        public bool LoadingIconActive
        {
            get { return loadingIconActive; }
            set { SetProperty(ref loadingIconActive, value); }
        }

        private bool geolocationEntryEnabled;
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
            FeatureID = AppConstants.NEW_ENTRY_ID;
            this.featureType = featureType;
            feature.Properties = new Dictionary<string, object>();

            DateEntry = DateTime.Now.ToShortDateString();
            GeolocationValues = new ObservableCollection<DisplayPosition>();
            GeolocationEntryEnabled = true;
            LoadingIconActive = false;

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
        }

        /// <summary>
        /// View-model constructor for viewing/editing existing entries.
        /// </summary>
        public FeatureDetailsViewModel(Feature feature)
        {
            InitCommandBindings();
            this.feature = feature;
            featureType = feature.Geometry.Type;

            if (feature.Properties.ContainsKey("id"))
            {
                FeatureID = (string)feature.Properties["id"];
            }

            if (feature.Properties.ContainsKey("name"))
            {
                Title = NameEntry = (string)feature.Properties["name"];
            }

            if (feature.Properties.ContainsKey("date"))
            {
                DateEntry = (string)feature.Properties["date"];
            }


            GeolocationValues = new ObservableCollection<DisplayPosition>();


            GeolocationEntryEnabled = true;

            if (feature.Properties.ContainsKey("metadataStringValue"))
            {
                MetadataStringEntry = (string)feature.Properties["metadataStringValue"];
            }

            if (feature.Properties.ContainsKey("metadataIntegerValue"))
            {
                int intval = Convert.ToInt32(feature.Properties["metadataIntegerValue"]);
                MetadataIntegerEntry = intval.ToString();
            }

            if (feature.Properties.ContainsKey("metadataFloatValue"))
            {
                float floatval = Convert.ToSingle(feature.Properties["metadataFloatValue"]);
                MetadataFloatEntry = floatval.ToString();
            }

            LoadingIconActive = false;

            int index = 1;

            //SWITCH TEMPLATE - maybe make method?
            switch (feature.Geometry.Type)
            {
                case GeoJSONType.Point:
                    Point point = (Point)feature.Geometry;
                    GeolocationValues.Add(new DisplayPosition(1, point.Coordinates));
                    break;
                case GeoJSONType.LineString:
                    LineString linestring = (LineString)feature.Geometry;
                    foreach (Position pos in linestring.Coordinates)
                    {
                        GeolocationValues.Add(new DisplayPosition(index, pos));
                        index++;
                    }
                    break;
                case GeoJSONType.Polygon:
                    Polygon polygon = (Polygon)feature.Geometry;
                    foreach (LineString ls in polygon.Coordinates)
                    {
                        foreach (Position pos in ls.Coordinates)
                        {
                            GeolocationValues.Add(new DisplayPosition(index, pos));
                            index++;
                        }
                    }
                    break;
                default:
                    //Unrecognised feature alert!!
                    break;

            }
            NumPointFields = GeolocationValues.Count + 1;
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<DisplayPosition>(async (point) => { await GetDataPoint(point); });
            AddPointCommand = new Command(() => AddPoint(1));
            DeletePointCommand = new Command<DisplayPosition>((item) => DeletePoint(item));
            ShareEntryCommand = new Command(async () => await FeatureStore.ExportFeatures(new ObservableCollection<Feature> { feature }));
            OnDoneTappedCommand = new Command(async () => await OnSaveUpdateActivated());
            OnCancelTappedCommand = new Command(async () => await OnDismiss(true));
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
            DisplayPosition convertedPoint = new DisplayPosition(0, location);
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
                GeolocationValues.Add(new DisplayPosition(GeolocationValues.Count + 1, "0", "0", "0"));
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
            if (GeolocationValues[0] == item)
            {
                NavigationService.ShowAlert("Cannot Remove Position", "All features must have at least one position", false);
                return;
            }
            IsBusy = true;
            GeolocationValues.Remove(item);
            for (int i = 0; i < GeolocationValues.Count; i++)
            {
                GeolocationValues[i].Index = i + 1;
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
            if (await TryParseSaveFeature())
            {
                await NavigationService.NavigateBack(true);
                IsBusy = false;
                return;
            }
            IsBusy = false;
        }

        private async Task<bool> TryParseSaveFeature()
        {
            try
            {
                switch (featureType)
                {
                    case GeoJSONType.Point:
                        if (GeolocationValues.Count != 1)
                        {
                            await NavigationService.ShowAlert("Unsupported Entry", "A point must only contain 1 data point.", false);
                            return false;
                        }
                        feature.Geometry = new Point(new Position(Convert.ToDouble(GeolocationValues[0].Longitude), Convert.ToDouble(GeolocationValues[0].Latitude), Convert.ToDouble(GeolocationValues[0].Altitude)));
                        break;
                    case GeoJSONType.LineString:
                        if (GeolocationValues.Count < 2)
                        {
                            await NavigationService.ShowAlert("Incomplete Entry", "A line must contain at least 2 data points.", false);
                            return false;
                        }

                        List<Position> posList = new List<Position>();
                        foreach (DisplayPosition pointValue in GeolocationValues)
                        {
                            posList.Add(new Position(Convert.ToDouble(pointValue.Longitude), Convert.ToDouble(pointValue.Latitude), Convert.ToDouble(pointValue.Altitude)));
                        }
                        feature.Geometry = new LineString(posList);
                        break;
                    case GeoJSONType.Polygon:
                        if (GeolocationValues.Count < 3)
                        {
                            await NavigationService.ShowAlert("Incomplete Entry", "A polygon must contain at least 4 data points.", false);
                            return false;
                        }

                        // This specific method of structuring points means that users will not be able to create multiple shapes in one polygon (whereas true GEOJSON allows that).
                        // This doesn't matter since our app interface can't allow for it yet.
                        List<DisplayPosition> ClosedPoly = new List<DisplayPosition>(GeolocationValues)
                        {
                            new DisplayPosition(GeolocationValues.Count + 1, GeolocationValues[0])
                        };
                        List<Position> polyPosList = new List<Position>();
                        foreach (DisplayPosition pointValue in ClosedPoly)
                        {
                            polyPosList.Add(new Position(Convert.ToDouble(pointValue.Longitude), Convert.ToDouble(pointValue.Latitude), Convert.ToDouble(pointValue.Altitude)));
                        }
                        List<LinearRing> polyLS = new List<LinearRing>() { new LinearRing(polyPosList) };
                        feature.Geometry = new Polygon(polyLS);
                        break;
                    default:
                        //Unrecognised feature alert!!
                        break;
                }
            }
            catch
            {
                await NavigationService.ShowAlert("Data Error", "Coordinate fields only support numeric values.", false);

                //undo close poly
                if (feature.Geometry.Type == GeoJSONType.Polygon)
                {
                    GeolocationValues.RemoveAt(GeolocationValues.Count - 1);
                }
                return false;
            }

            //Metadata
            addprops("metadataStringValue", MetadataStringEntry);
            try
            {
                if (!string.IsNullOrEmpty(MetadataIntegerEntry))
                {
                    addprops("metadataIntegerValue", Convert.ToInt32(MetadataIntegerEntry));
                }
                else
                {
                    addprops("metadataIntegerValue", null);
                }

                if (!string.IsNullOrEmpty(MetadataFloatEntry))
                {
                    addprops("metadataFloatValue", Convert.ToSingle(MetadataFloatEntry));
                }
                else
                {
                    addprops("metadataFloatValue", null);
                }
            }
            catch
            {
                await NavigationService.ShowAlert("Data Error", "Integer and float fields only support numeric values.", false);
                return false;
            }


            if (string.IsNullOrEmpty(NameEntry))
            {
                addprops("name", feature.Geometry.Type.ToString());
            }
            else
            {
                addprops("name", NameEntry);
            }

            addprops("id", FeatureID);
            addprops("date", DateTime.Parse(DateEntry).ToShortDateString());
            addprops("author", Preferences.Get("UserID", "Groundsman"));

            bool success;
            if (FeatureID == AppConstants.NEW_ENTRY_ID)
            {
                success = await FeatureStore.AddItemAsync(feature);
            }
            else
            {
                success = await FeatureStore.UpdateItemAsync(feature);
            }

            return success;
        }

        private void addprops(string key, object value)
        {
            if (feature.Properties.ContainsKey(key))
            {
                feature.Properties[key] = value;
            }
            else
            {
                feature.Properties.Add(key, value);
            }
        }

    }
}
