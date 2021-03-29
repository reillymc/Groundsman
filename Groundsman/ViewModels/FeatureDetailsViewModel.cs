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
                if (float.IsNormal(floatval))
                {
                    MetadataFloatEntry = floatval.ToString();
                }
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
                    ShowAddButton = true;
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
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<DisplayPosition>(async (point) => { await GetDataPoint(point); });
            AddPointCommand = new Command(() => AddPoint(1));
            DeletePointCommand = new Command<DisplayPosition>((item) => DeletePoint(item));
            ShareEntryCommand = new Command(async () => await ShareFeature());
            OnDoneTappedCommand = new Command(async () => await OnSaveUpdateActivated());
            OnCancelTappedCommand = new Command(async () => await OnDismiss(true));
        }

        private async Task ShareFeature()
        {
            if (IsBusy) return;

            IsBusy = true;
            if (await TryParseSaveFeature())
            {
                await FeatureStore.ExportFeatures(new List<Feature> { feature });
                IsBusy = false;
                return;
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
                GeolocationValues.Add(new DisplayPosition(GeolocationValues.Count + 1, "", "", ""));
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
                            await NavigationService.ShowAlert("Unsupported Entry", "A point must only contain 1 positions.", false);
                            return false;
                        }
                        feature.Geometry = new Point(ConvertPosition(GeolocationValues[0]));
                        break;

                    case GeoJSONType.LineString:
                        if (GeolocationValues.Count < 2)
                        {
                            await NavigationService.ShowAlert("Incomplete Entry", "A line must contain at least 2 positions.", false);
                            return false;
                        }
                        List<Position> posList = new List<Position>();
                        foreach (DisplayPosition pointValue in GeolocationValues)
                        {
                            posList.Add(ConvertPosition(pointValue));
                        }
                        feature.Geometry = new LineString(posList);
                        break;
                    case GeoJSONType.Polygon:
                        if (GeolocationValues.Count < 3)
                        {
                            await NavigationService.ShowAlert("Incomplete Entry", "A polygon must contain at least 4 positions.", false);
                            return false;
                        }

                        // This specific method of structuring points means that users will not be able to create multiple shapes in one polygon (whereas true GEOJSON allows that).
                        // This doesn't matter since our app interface can't allow for it yet.
                        List<Position> polyPosList = new List<Position>();
                        foreach (DisplayPosition pointValue in GeolocationValues)
                        {
                            polyPosList.Add(ConvertPosition(pointValue));
                        }
                        // Close polygon with duplicated first feature
                        polyPosList.Add(polyPosList[0]);

                        feature.Geometry = new Polygon(new List<LinearRing>() { new LinearRing(polyPosList) });
                        break;
                    default:
                        //Unrecognised feature alert!!
                        break;
                }
            }
            catch (Exception ex)
            {
                await NavigationService.ShowAlert("Saving Error", $"{ex.Message}", false);
                return false;
            }

            //Metadata
            if (string.IsNullOrEmpty("metadataStringValue"))
            {
                feature.Properties.Remove("metadataStringValue");
            }
            else
            {
                feature.Properties["metadataStringValue"] = MetadataStringEntry;
            }

            try
            {
                if (string.IsNullOrEmpty(MetadataIntegerEntry))
                {
                    feature.Properties.Remove("metadataIntegerValue");
                }
                else
                {
                    feature.Properties["metadataIntegerValue"] = Convert.ToInt32(MetadataIntegerEntry);
                }

                if (string.IsNullOrEmpty(MetadataFloatEntry))
                {
                    feature.Properties.Remove("metadataFloatValue");
                }
                else
                {
                    feature.Properties["metadataFloatValue"] = Convert.ToSingle(MetadataFloatEntry);
                }
            }
            catch
            {
                await NavigationService.ShowAlert("Data Error", "Integer and float fields only support numeric values.", false);
                return false;
            }

            feature.Properties["name"] = string.IsNullOrEmpty(NameEntry) ? feature.Geometry.Type.ToString() : NameEntry;
            feature.Properties["id"] = FeatureID;
            feature.Properties["date"] = DateTime.Parse(DateEntry).ToShortDateString();
            feature.Properties["author"] = Preferences.Get("UserID", "Groundsman");

            return FeatureID == AppConstants.NEW_ENTRY_ID ? await FeatureStore.AddItemAsync(feature) : await FeatureStore.UpdateItemAsync(feature);
        }

        private Position ConvertPosition(DisplayPosition displayPosition)
        {
            try
            {
                double longitude = string.IsNullOrEmpty(displayPosition.Longitude) ? 0 : Convert.ToDouble(displayPosition.Longitude);
                double latitude = string.IsNullOrEmpty(displayPosition.Latitude) ? 0 : Convert.ToDouble(displayPosition.Latitude);
                return string.IsNullOrEmpty(displayPosition.Altitude) ? new Position(longitude, latitude) : new Position(longitude, latitude, Convert.ToDouble(displayPosition.Altitude));
            }
            catch
            {
                throw new ArgumentException("Coordinates may only contain numeric values");
            }
        }
    }
}
