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
        public ICommand AddMetadataFieldCommand { get; set; }
        public ICommand DeleteMetadataFieldCommand { get; set; }
        public ICommand OnSaveUpdatedCommand { get; set; }
        public ICommand ShareEntryCommand { get; set; }
        public ICommand ClosePolyCommand { get; set; }

        // Property binding to determine if the delete button for metadata fields is visible, which is based on the type of this entry.
        public bool ShowPointDeleteBtn { get { return _numPointFields > minPoints; } }
        private int minPoints;

        private Feature thisFeature = new Feature();
        // A reference to this entry's ID.
        private string thisEntryID;

        // A reference to this entry's type of structure.
        private string thisEntryType;

        private bool _isBusy;

        public ObservableCollection<DisplayPoint> GeolocationValues { get; set; }

        private string _dateEntry;
        public string DateEntry
        {
            get { return _dateEntry; }
            set
            {
                _dateEntry = value;
                OnPropertyChanged();
            }
        }

        private string _nameEntry;
        public string NameEntry
        {
            get { return _nameEntry; }
            set
            {
                _nameEntry = value;
                OnPropertyChanged();
            }
        }

        private bool _loadingIconActive;
        public bool LoadingIconActive
        {
            get { return _loadingIconActive; }
            set
            {
                _loadingIconActive = value;
                OnPropertyChanged();
            }
        }

        private bool _geolocationEntryEnabled;
        public bool GeolocationEntryEnabled
        {
            get { return _geolocationEntryEnabled; }
            set
            {
                _geolocationEntryEnabled = value;
                OnPropertyChanged();
            }
        }

        private int _numPointFields;
        public int NumPointFields
        {
            get { return _numPointFields; }
            set
            {
                _numPointFields = value;
                OnPropertyChanged();
                OnPropertyChanged("ShowPointDeleteBtn");
            }
        }

        private string _metadataStringEntry;
        public string MetadataStringEntry
        {
            get { return _metadataStringEntry; }
            set
            {
                _metadataStringEntry = value;
                OnPropertyChanged();
            }
        }

        private int _metadataIntegerEntry;
        public int MetadataIntegerEntry
        {
            get { return _metadataIntegerEntry; }
            set
            {
                _metadataIntegerEntry = value;
                OnPropertyChanged();
            }
        }

        private string _metadataFloatEntry;
        public string MetadataFloatEntry
        {
            get { return _metadataFloatEntry; }
            set
            {
                _metadataFloatEntry = value;
                OnPropertyChanged();
            }
        }

        private string _typeIconPath;

        /// <summary>
        /// View-model constructor for adding new entries.
        /// </summary>
        public FeatureDetailsViewModel(string entryType)
        {
            thisEntryType = entryType;
            thisEntryID = AppConstants.NEW_ENTRY_ID;

            DateEntry = DateTime.Now.ToShortDateString();

            // Add the minimum number of points necessary for the chosen type.
            {
                switch (entryType)
                {
                    case "Point":
                        minPoints = 1;
                        break;
                    case "LineString":
                        minPoints = 2;
                        break;
                    case "Polygon":
                        minPoints = 4;
                        break;
                }

                GeolocationValues = new ObservableCollection<DisplayPoint>();
                for (int i = 0; i < minPoints; i++)
                {
                    AddPoint();
                }
            }

            GeolocationEntryEnabled = true;
            LoadingIconActive = false;

            InitCommandBindings();
        }

        /// <summary>
        /// View-model constructor for viewing/editing existing entries.
        /// </summary>
        public FeatureDetailsViewModel(Feature data)
        {
            thisFeature = data;
            thisEntryType = data.geometry.type;
            thisEntryID = data.properties.id;
            switch (thisEntryType)
            {
                case "Point":
                    minPoints = 1;
                    break;
                case "LineString":
                    minPoints = 2;
                    break;
                case "Polygon":
                    minPoints = 4;
                    break;
            }

            NameEntry = data.properties.name;
            DateEntry = DateTime.Parse(data.properties.date).ToShortDateString();

            GeolocationValues = new ObservableCollection<DisplayPoint>();

            foreach (Point pointValue in data.properties.xamarincoordinates)
            {
                DisplayPoint convertedPoint = new DisplayPoint(pointValue.Latitude.ToString(), pointValue.Longitude.ToString(), pointValue.Altitude.ToString());
                GeolocationValues.Add(convertedPoint);
            }

            GeolocationEntryEnabled = true;

            MetadataStringEntry = data.properties.metadataStringValue;
            MetadataIntegerEntry = data.properties.metadataIntegerValue;
            MetadataFloatEntry = data.properties.metadataFloatValue.ToString();

            _typeIconPath = data.properties.typeIconPath;

            LoadingIconActive = false;
            NumPointFields = data.properties.xamarincoordinates.Count;
            InitCommandBindings();
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<DisplayPoint>(async (point) => { await GetDataPoint(point); });

            AddPointCommand = new Command(() => AddPoint());
            DeletePointCommand = new Command<DisplayPoint>((item) => DeletePoint(item));

            ShareEntryCommand = new Command(async () => await featureStore.ExportFeatures(new ObservableCollection<Feature> { thisFeature }));

            OnSaveUpdatedCommand = new Command(async () => await OnSaveUpdateActivated());

            ClosePolyCommand = new Command(() => ClosePoly());
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
            DisplayPoint convertedPoint = new DisplayPoint(location.Latitude.ToString(), location.Longitude.ToString(), location.Altitude.ToString());
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
        private void AddPoint()
        {
            if (_isBusy) return;
            _isBusy = true;

            GeolocationValues.Add(new DisplayPoint("0", "0", "0"));
            NumPointFields++;
            _isBusy = false;
        }

        /// <summary>
        /// Deletes a geolocation point from the list.
        /// </summary>
        /// <param name="item">Item to delete</param>
        private void DeletePoint(DisplayPoint item)
        {
            if (_isBusy) return;
            _isBusy = true;

            GeolocationValues.Remove(item);
            NumPointFields--;
            _isBusy = false;
        }

        /// <summary>
        /// Saves a new or edited feature to the embedded file.
        /// </summary>
        async Task OnSaveUpdateActivated()
        {
            if (_isBusy) return;
            _isBusy = true;

            // Do validation checks here.
            if (await FeatureEntryIsValid() == false)
            {
                _isBusy = false;
                return;
            }

            Feature featureToSave = CreateFeatureFromInput();
            bool success;
            if (featureToSave.properties.id == AppConstants.NEW_ENTRY_ID)
            {
                success = await featureStore.AddItemAsync(featureToSave);

            }
            else
            {
                success = await featureStore.UpdateItemAsync(featureToSave);
            }
            if (!success)
            {
                await HomePage.Instance.DisplayAlert("Save Error", "Feature not saved.", "OK");
            }
            await HomePage.Instance.Navigation.PopToRootAsync();

            _isBusy = false;
        }

        /// <summary>
        /// Creates a feature object based on the view-model data of this feature entry.
        /// </summary>
        /// <returns>A feature object formed from input values</returns>
        private Feature CreateFeatureFromInput()
        {
            Feature feature = new Feature
            {
                type = "Feature",
                properties = new Properties()
            };

            // A new entry will have an ID of NEW_ENTRY_ID as assigned from the constructor,
            // otherwise an ID will already be set for editing entries.
            feature.properties.id = thisEntryID;
            feature.properties.author = Preferences.Get("UserID", "Groundsman");

            // Feature type (Point, Line, Polygon).
            feature.geometry = new Geometry
            {
                type = thisEntryType
            };

            // Name and date of the feature.
            if (string.IsNullOrEmpty(NameEntry))
            {
                feature.properties.name = "Unnamed " + feature.geometry.type;
            }
            else
            {
                feature.properties.name = NameEntry;
            }
            feature.properties.date = DateTime.Parse(DateEntry).ToShortDateString();

            // Metadata fields.
            feature.properties.metadataStringValue = MetadataStringEntry;
            feature.properties.metadataIntegerValue = MetadataIntegerEntry;
            feature.properties.metadataFloatValue = Convert.ToSingle(MetadataFloatEntry);

            feature.properties.xamarincoordinates = new List<Point>();

            foreach (DisplayPoint pointValue in GeolocationValues)
            {
                Point convertedPoint = new Point(Convert.ToDouble(pointValue.Latitude), Convert.ToDouble(pointValue.Longitude), Convert.ToDouble(pointValue.Altitude));
                feature.properties.xamarincoordinates.Add(convertedPoint);
            }

            switch (feature.geometry.type)
            {
                case "Point":
                    feature.properties.typeIconPath = "point_icon.png";
                    break;
                case "LineString":
                    feature.properties.typeIconPath = "line_icon.png";
                    break;
                case "Polygon":
                    feature.properties.typeIconPath = "area_icon.png";
                    break;
                default:
                    break;
            }
            // Converts our xamarin coordinate data back into a valid geojson structure.
            {
                switch (thisEntryType)
                {
                    case "Point":
                        feature.geometry.coordinates = new List<object>() {
                        GeolocationValues[0].Longitude,
                        GeolocationValues[0].Latitude,
                        GeolocationValues[0].Altitude };
                        break;
                    case "LineString":
                        feature.geometry.coordinates = new List<object>(GeolocationValues.Count);
                        for (int i = 0; i < GeolocationValues.Count; i++)
                        {
                            feature.geometry.coordinates.Add(new JArray(new double[3] {
                            Convert.ToDouble(GeolocationValues[i].Longitude),
                            Convert.ToDouble(GeolocationValues[i].Latitude),
                            Convert.ToDouble(GeolocationValues[i].Altitude)
                            }));
                        }

                        break;
                    case "Polygon":
                        // This specific method of structuring points means that users will not
                        // be able to create multiple shapes in one polygon (whereas true GEOJSON allows that).
                        // This doesn't matter since our app interface can't allow for it anyway.
                        feature.geometry.coordinates = new List<object>(GeolocationValues.Count);
                        List<object> innerPoints = new List<object>(GeolocationValues.Count);
                        for (int i = 0; i < GeolocationValues.Count; i++)
                        {
                            innerPoints.Add(new JArray(new double[3] {
                            Convert.ToDouble(GeolocationValues[i].Longitude),
                            Convert.ToDouble(GeolocationValues[i].Latitude),
                            Convert.ToDouble(GeolocationValues[i].Altitude)
                            }));
                        }
                        feature.geometry.coordinates.Add(innerPoints);
                        break;
                }
            }
            return feature;
        }

        /// <summary>
        /// Performs validation checks on the data in the form.
        /// </summary>
        /// <returns>True if the form contains valid data.</returns>
        private async Task<bool> FeatureEntryIsValid()
        {
            /// Begin validation checks.
            switch (thisEntryType)
            {
                case "Polygon":
                    if (GeolocationValues.Count < 4)
                    {
                        await HomePage.Instance.DisplayAlert("Incomplete Entry", "A polygon must contain at least 4 data points.", "OK");
                        return false;
                    }

                    // Check if first and last points of the polygon have the same lat/long values.
                    {
                        string firstLatitude = GeolocationValues[0].Latitude;
                        string lastLatitude = GeolocationValues[GeolocationValues.Count - 1].Latitude;
                        string firstLongitude = GeolocationValues[0].Longitude;
                        string lastLongitude = GeolocationValues[GeolocationValues.Count - 1].Longitude;

                        if (firstLatitude != lastLatitude || firstLongitude != lastLongitude)
                        {
                            await HomePage.Instance.DisplayAlert("Incomplete Entry", "The first and last points of a polygon must match.", "OK");
                            return false;
                        }
                    }

                    break;
                case "LineString":
                    if (GeolocationValues.Count < 2)
                    {
                        await HomePage.Instance.DisplayAlert("Incomplete Entry", "A line must contain at least 2 data points.", "OK");
                        return false;
                    }

                    break;
                case "Point":
                    if (GeolocationValues.Count != 1)
                    {
                        await HomePage.Instance.DisplayAlert("Unsupported Entry", "A point must only contain 1 data point.", "OK");
                        return false;
                    }

                    break;
            }

            return true;
        }

        private void ClosePoly()
        {
            string latFist = GeolocationValues[0].Latitude;
            string lonFist = GeolocationValues[0].Longitude;
            string altFist = GeolocationValues[0].Altitude;

            if (_isBusy) return;
            _isBusy = true;

            GeolocationValues.Add(new DisplayPoint(latFist, lonFist, altFist));
            NumPointFields++;

            _isBusy = false;
        }

    }


}
