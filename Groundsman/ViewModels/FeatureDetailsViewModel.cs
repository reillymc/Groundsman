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
        public ICommand OnSaveUpdatedCommand { get; set; }
        public ICommand OnDismissCommand { get; set; }
        public ICommand ShareEntryCommand { get; set; }

        private Feature feature = new Feature { };
        public ObservableCollection<DisplayPoint> GeolocationValues { get; set; }

        public string DateEntry { get; set; }
        public string NameEntry { get; set; }
        public string MetadataStringEntry { get; set; }
        public string MetadataIntegerEntry { get; set; }
        public string MetadataFloatEntry { get; set; }

        public bool LoadingIconActive { get; set; }
        public bool GeolocationEntryEnabled { get; set; }
        public bool ShowAddButton { get; set; }
        public bool ShowClosePolygon { get; set; }
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
        public FeatureDetailsViewModel(FeatureType featureType)
        {
            feature.type = "Feature";
            feature.properties = new Properties
            {
                xamarincoordinates = new List<Point>(),
                id = AppConstants.NEW_ENTRY_ID
            };
            feature.geometry = new Geometry
            {
                type = featureType
            };

            DateEntry = DateTime.Now.ToShortDateString();
            GeolocationValues = new ObservableCollection<DisplayPoint>();
            GeolocationEntryEnabled = true;
            LoadingIconActive = false;

            InitCommandBindings();

            switch (featureType)
            {
                case FeatureType.Point:
                    Title = "New Point";
                    AddPoint(1);
                    break;
                case FeatureType.LineString:
                    Title = "New Line";
                    ShowAddButton = true;
                    AddPoint(2);
                    break;
                case FeatureType.Polygon:
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
            Title = feature.properties.name;
            switch (feature.geometry.type)
            {
                case FeatureType.LineString:
                    ShowAddButton = true;
                    break;
                case FeatureType.Polygon:
                    ShowAddButton = true;
                    ShowClosePolygon = true;
                    break;
            }

            NameEntry = feature.properties.name;
            DateEntry = DateTime.Parse(feature.properties.date).ToShortDateString();

            GeolocationValues = new ObservableCollection<DisplayPoint>();

            for (int i = 0; i < feature.properties.xamarincoordinates.Count; i++)
            {
                DisplayPoint convertedPoint = new DisplayPoint(i + 1, feature.properties.xamarincoordinates[i].Latitude.ToString(), feature.properties.xamarincoordinates[i].Longitude.ToString(), feature.properties.xamarincoordinates[i].Altitude.ToString());
                GeolocationValues.Add(convertedPoint);
            }

            GeolocationEntryEnabled = true;

            MetadataStringEntry = feature.properties.metadataStringValue;
            MetadataIntegerEntry = feature.properties.metadataIntegerValue.ToString(); ;
            MetadataFloatEntry = feature.properties.metadataFloatValue.ToString();

            LoadingIconActive = false;
            NumPointFields = GeolocationValues.Count + 1;
            InitCommandBindings();
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<DisplayPoint>(async (point) => { await GetDataPoint(point); });
            AddPointCommand = new Command(() => AddPoint(1));
            DeletePointCommand = new Command<DisplayPoint>((item) => DeletePoint(item));
            ShareEntryCommand = new Command(async () => await featureStore.ExportFeatures(new ObservableCollection<Feature> { feature }));
            OnSaveUpdatedCommand = new Command(async () => await OnSaveUpdateActivated());
            OnDismissCommand = new Command(async () => await OnDismiss(true));
        }

        /// <summary>
        /// Queries the current device's location coordinates
        /// </summary>
        /// <param name="point">Point to set GPS data to.</param>
        private async Task GetDataPoint(DisplayPoint point)
        {
            GeolocationEntryEnabled = false;
            LoadingIconActive = true;
            Point location = await HelperServices.GetGeoLocation();
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
            IsBusy = true;
            if (ShowAddButton == false)
            {
                HomePage.Instance.DisplayAlert("Cannot remove point", "A point feature must have one data point", "Ok");
                return;
            }
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
                await navigationService.NavigateBack(true);
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
                switch (feature.geometry.type)
                {
                    case FeatureType.Point:
                        if (GeolocationValues.Count != 1)
                        {
                            await HomePage.Instance.DisplayAlert("Unsupported Entry", "A point must only contain 1 data point.", "OK");
                            return false;
                        }
                        feature.properties.xamarincoordinates.Clear();
                        feature.geometry.coordinates = new List<object>() {
                        Convert.ToDouble(GeolocationValues[0].Longitude),
                        Convert.ToDouble(GeolocationValues[0].Latitude),
                        Convert.ToDouble(GeolocationValues[0].Altitude)
                        };
                        feature.properties.xamarincoordinates.Add(new Point(Convert.ToDouble(GeolocationValues[0].Latitude), Convert.ToDouble(GeolocationValues[0].Longitude), Convert.ToDouble(GeolocationValues[0].Altitude)));
                        break;
                    case FeatureType.LineString:
                        if (GeolocationValues.Count < 2)
                        {
                            await HomePage.Instance.DisplayAlert("Incomplete Entry", "A line must contain at least 2 data points.", "OK");
                            return false;
                        }
                        feature.properties.xamarincoordinates.Clear();
                        feature.geometry.coordinates = new List<object>(GeolocationValues.Count);
                        foreach (DisplayPoint pointValue in GeolocationValues)
                        {
                            feature.geometry.coordinates.Add(new JArray(new double[3] {
                            Convert.ToDouble(pointValue.Longitude),
                            Convert.ToDouble(pointValue.Latitude),
                            Convert.ToDouble(pointValue.Altitude)
                            }));

                            feature.properties.xamarincoordinates.Add(new Point(Convert.ToDouble(pointValue.Latitude), Convert.ToDouble(pointValue.Longitude), Convert.ToDouble(pointValue.Altitude)));
                        }
                        break;
                    case FeatureType.Polygon:
                        if (GeolocationValues.Count < 3)
                        {
                            await HomePage.Instance.DisplayAlert("Incomplete Entry", "A polygon must contain at least 4 data points.", "OK");
                            return false;
                        }
                        
                        // This specific method of structuring points means that users will not be able to create multiple shapes in one polygon (whereas true GEOJSON allows that).
                        // This doesn't matter since our app interface can't allow for it anyway.
                        feature.geometry.coordinates = new List<object>(GeolocationValues.Count);
                        List<object> innerPoints = new List<object>(GeolocationValues.Count);

                        List<DisplayPoint> ClosedPoly = new List<DisplayPoint>(GeolocationValues)
                        {
                            new DisplayPoint(GeolocationValues.Count + 1, GeolocationValues[0].Latitude, GeolocationValues[0].Longitude, GeolocationValues[0].Altitude)
                        };

                        feature.properties.xamarincoordinates.Clear();
                        foreach (DisplayPoint pointValue in ClosedPoly)
                        {
                            innerPoints.Add(new JArray(new double[3] {
                            Convert.ToDouble(pointValue.Longitude),
                            Convert.ToDouble(pointValue.Latitude),
                            Convert.ToDouble(pointValue.Altitude)
                            }));

                            feature.properties.xamarincoordinates.Add(new Point(Convert.ToDouble(pointValue.Latitude), Convert.ToDouble(pointValue.Longitude), Convert.ToDouble(pointValue.Altitude)));

                        }
                        feature.geometry.coordinates.Add(innerPoints);
                        break;
                }
            }
            catch
            {
                await HomePage.Instance.DisplayAlert("Data Error", "Coordinate fields only support numeric values.", "Ok");

                //undo close poly
                if (feature.geometry.type == FeatureType.Polygon)
                {
                    GeolocationValues.RemoveAt(GeolocationValues.Count - 1);
                }
                return false;
            }

            //Metadata
            feature.properties.metadataStringValue = MetadataStringEntry;
            try
            {
                if (MetadataIntegerEntry != null)
                {
                    feature.properties.metadataIntegerValue = Convert.ToInt32(MetadataIntegerEntry);
                }
                else
                {
                    feature.properties.metadataIntegerValue = null;
                }
                if (MetadataFloatEntry != null)
                {
                    feature.properties.metadataFloatValue = Convert.ToSingle(MetadataFloatEntry);
                }
                else
                {
                    feature.properties.metadataFloatValue = null;
                }
            }
            catch
            {
                await HomePage.Instance.DisplayAlert("Data Error", "Integer and float fields only support numeric values.", "Ok");
                return false;
            }

            feature.properties.author = Preferences.Get("UserID", "Groundsman");
            if (string.IsNullOrEmpty(NameEntry))
            {
                feature.properties.name = "Unnamed " + feature.geometry.type;
            }
            else
            {
                feature.properties.name = NameEntry;
            }
            feature.properties.date = DateTime.Parse(DateEntry).ToShortDateString();


            bool success;
            if (feature.properties.id == AppConstants.NEW_ENTRY_ID)
            {
                success = await featureStore.AddItemAsync(feature);
            }
            else
            {
                success = await featureStore.UpdateItemAsync(feature);
            }
            return success;
        }
    }
}