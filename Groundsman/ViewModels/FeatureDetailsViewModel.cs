using Groundsman.Models;
using Groundsman.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
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

        private string FeatureID;
        List<Position> Xamarincoordinates;
        public ObservableCollection<DisplayPoint> GeolocationValues { get; set; }

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
            feature.Type = GeoJSONType.Feature;
            Xamarincoordinates = new List<Position>();

            FeatureID = AppConstants.NEW_ENTRY_ID;
            feature.Geometry = new Geometry
            {
                Type = featureType
            };
            feature.Properties = new Dictionary<string, object>();

            DateEntry = DateTime.Now.ToShortDateString();
            GeolocationValues = new ObservableCollection<DisplayPoint>();
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
            this.feature = feature;

            if (feature.Properties.ContainsKey("id"))
            {
                FeatureID = (string)feature.Properties["id"];
            }

            if (feature.Properties.ContainsKey("xamarincoordinates"))
            {
                Xamarincoordinates = (List<Position>)feature.Properties["xamarincoordinates"];
            }

            if (feature.Properties.ContainsKey("name"))
            {
                Title = NameEntry = (string)feature.Properties["name"];
            }

            if (feature.Properties.ContainsKey("date"))
            {
                DateEntry = (string)feature.Properties["date"];
            }


            GeolocationValues = new ObservableCollection<DisplayPoint>();

            for (int i = 0; i < Xamarincoordinates.Count; i++)
            {
                DisplayPoint convertedPoint = new DisplayPoint(i + 1, Xamarincoordinates[i].Latitude.ToString(), Xamarincoordinates[i].Longitude.ToString(), Xamarincoordinates[i].Altitude.ToString());
                GeolocationValues.Add(convertedPoint);
            }

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


            InitCommandBindings();


            switch (feature.Geometry.Type)
            {
                case GeoJSONType.LineString:
                    ShowAddButton = true;
                    break;
                case GeoJSONType.Polygon:
                    ShowAddButton = true;
                    ShowClosePolygon = true;
                    if (GeolocationValues[0].Latitude == GeolocationValues[GeolocationValues.Count - 1].Latitude && GeolocationValues[0].Longitude == GeolocationValues[GeolocationValues.Count - 1].Longitude && GeolocationValues[0].Altitude == GeolocationValues[GeolocationValues.Count - 1].Altitude)
                    {
                        GeolocationValues.RemoveAt(GeolocationValues.Count - 1);
                    }
                    break;
            }
            NumPointFields = GeolocationValues.Count + 1;
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<DisplayPoint>(async (point) => { await GetDataPoint(point); });
            AddPointCommand = new Command(() => AddPoint(1));
            DeletePointCommand = new Command<DisplayPoint>((item) => DeletePoint(item));
            ShareEntryCommand = new Command(async () => await FeatureStore.ExportFeatures(new ObservableCollection<Feature> { feature }));
            OnDoneTappedCommand = new Command(async () => await OnSaveUpdateActivated());
            OnCancelTappedCommand = new Command(async () => await OnDismiss(true));
        }

        /// <summary>
        /// Queries the current device's location coordinates
        /// </summary>
        /// <param name="point">Point to set GPS data to.</param>
        private async Task GetDataPoint(DisplayPoint point)
        {
            GeolocationEntryEnabled = false;
            LoadingIconActive = true;

            Position location = await HelperServices.GetGeoLocation();
            DisplayPoint convertedPoint = new DisplayPoint(0, location.Latitude.ToString(), location.Longitude.ToString(), location.Altitude.ToString());
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
                GeolocationValues.Add(new DisplayPoint(GeolocationValues.Count + 1, "0", "0", "0"));
                NumPointFields++;
            }
            IsBusy = false;
        }

        /// <summary>
        /// Deletes a geolocation point from the list.
        /// </summary>
        /// <param name="item">Item to delete</param>
        private void DeletePoint(DisplayPoint item)
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
            // Specific requirements and coordinate parsing
            try
            {
                switch (feature.Geometry.Type)
                {
                    case GeoJSONType.Point:
                        if (GeolocationValues.Count != 1)
                        {
                            await NavigationService.ShowAlert("Unsupported Entry", "A point must only contain 1 data point.", false);
                            return false;
                        }
                        Xamarincoordinates.Clear();
                        feature.Geometry.Coordinates = new List<object>() {
                        Convert.ToDouble(GeolocationValues[0].Longitude),
                        Convert.ToDouble(GeolocationValues[0].Latitude),
                        Convert.ToDouble(GeolocationValues[0].Altitude)
                        };
                        Xamarincoordinates.Add(new Position(Convert.ToDouble(GeolocationValues[0].Latitude), Convert.ToDouble(GeolocationValues[0].Longitude), Convert.ToDouble(GeolocationValues[0].Altitude)));
                        break;
                    case GeoJSONType.LineString:
                        if (GeolocationValues.Count < 2)
                        {
                            await NavigationService.ShowAlert("Incomplete Entry", "A line must contain at least 2 data points.", false);
                            return false;
                        }
                        Xamarincoordinates.Clear();
                        feature.Geometry.Coordinates = new List<object>(GeolocationValues.Count);
                        foreach (DisplayPoint pointValue in GeolocationValues)
                        {
                            feature.Geometry.Coordinates.Add(new JArray(new double[3] {
                            Convert.ToDouble(pointValue.Longitude),
                            Convert.ToDouble(pointValue.Latitude),
                            Convert.ToDouble(pointValue.Altitude)
                            }));

                            Xamarincoordinates.Add(new Position(Convert.ToDouble(pointValue.Latitude), Convert.ToDouble(pointValue.Longitude), Convert.ToDouble(pointValue.Altitude)));
                        }
                        break;
                    case GeoJSONType.Polygon:
                        if (GeolocationValues.Count < 3)
                        {
                            await NavigationService.ShowAlert("Incomplete Entry", "A polygon must contain at least 4 data points.", false);
                            return false;
                        }

                        // This specific method of structuring points means that users will not be able to create multiple shapes in one polygon (whereas true GEOJSON allows that).
                        // This doesn't matter since our app interface can't allow for it anyway.
                        feature.Geometry.Coordinates = new List<object>(GeolocationValues.Count);
                        List<object> innerPoints = new List<object>(GeolocationValues.Count);

                        List<DisplayPoint> ClosedPoly = new List<DisplayPoint>(GeolocationValues)
                        {
                            new DisplayPoint(GeolocationValues.Count + 1, GeolocationValues[0].Latitude, GeolocationValues[0].Longitude, GeolocationValues[0].Altitude)
                        };

                        Xamarincoordinates.Clear();
                        foreach (DisplayPoint pointValue in ClosedPoly)
                        {
                            innerPoints.Add(new JArray(new double[3] {
                            Convert.ToDouble(pointValue.Longitude),
                            Convert.ToDouble(pointValue.Latitude),
                            Convert.ToDouble(pointValue.Altitude)
                            }));

                            Xamarincoordinates.Add(new Position(Convert.ToDouble(pointValue.Latitude), Convert.ToDouble(pointValue.Longitude), Convert.ToDouble(pointValue.Altitude)));

                        }
                        feature.Geometry.Coordinates.Add(innerPoints);
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
                addprops("name", feature.Geometry.Type);
            }
            else
            {
                addprops("name", NameEntry);
            }

            addprops("xamarincoordinates", Xamarincoordinates);
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
