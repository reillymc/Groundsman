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
        public ICommand OnDismissCommand { get; set; }
        public ICommand ShareEntryCommand { get; set; }
        public ICommand ClosePolyCommand { get; set; }

        // Property binding to determine if the delete button for metadata fields is visible, which is based on the type of this entry.
        public bool ShowPointDeleteBtn { get { return _numPointFields > minPoints; } }
        private int minPoints;

        private Feature thisFeature = new Feature();
        // A reference to this entry's ID.
        private string thisEntryID;

        // A reference to this entry's type of structure.
        private FeatureType thisEntryType;

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
            get { return _numPointFields + 1; }
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

        private string _metadataIntegerEntry;
        public string MetadataIntegerEntry
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
        public bool ShowAddButton { get; set; }
        public bool ShowCloseLine { get; set; }
        /// <summary>
        /// View-model constructor for adding new entries.
        /// </summary>
        public FeatureDetailsViewModel(FeatureType featureType)
        {
            switch (featureType)
            {
                case FeatureType.Point:
                    Title = "New Point";
                    break;
                case FeatureType.LineString:
                    ShowAddButton = true;
                    Title = "New Line";
                    break;
                case FeatureType.Polygon:
                    Title = "New Polygon";
                    ShowAddButton = true;
                    ShowCloseLine = true;
                    break;
            }

            thisEntryID = AppConstants.NEW_ENTRY_ID;
            thisEntryType = featureType;
            DateEntry = DateTime.Now.ToShortDateString();

            GeolocationValues = new ObservableCollection<DisplayPoint>();
            AddPoint();



            GeolocationEntryEnabled = true;
            LoadingIconActive = false;

            InitCommandBindings();
        }

        /// <summary>
        /// View-model constructor for viewing/editing existing entries.
        /// </summary>
        public FeatureDetailsViewModel(Feature data)
        {
            Title = data.properties.name;
            switch (data.geometry.type)
            {
                case FeatureType.LineString:
                    ShowAddButton = true;
                    break;
                case FeatureType.Polygon:
                    ShowAddButton = true;
                    ShowCloseLine = true;
                    break;
            }

            thisFeature = data;
            thisEntryType = data.geometry.type;
            thisEntryID = data.properties.id;
            NameEntry = data.properties.name;
            DateEntry = DateTime.Parse(data.properties.date).ToShortDateString();

            GeolocationValues = new ObservableCollection<DisplayPoint>();

            for (int i = 0; i < data.properties.xamarincoordinates.Count; i++)
            {
                DisplayPoint convertedPoint = new DisplayPoint(i + 1, data.properties.xamarincoordinates[i].Latitude.ToString(), data.properties.xamarincoordinates[i].Longitude.ToString(), data.properties.xamarincoordinates[i].Altitude.ToString());
                GeolocationValues.Add(convertedPoint);
            }

            GeolocationEntryEnabled = true;

            MetadataStringEntry = data.properties.metadataStringValue;
            MetadataIntegerEntry = data.properties.metadataIntegerValue.ToString(); ;
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

            OnDismissCommand = new Command(async () => await OnDismiss());

        }

        private async Task OnDismiss()
        {
            await navigationService.NavigateBack(true);
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
        private void AddPoint()
        {
            if (_isBusy) return;
            _isBusy = true;

            GeolocationValues.Add(new DisplayPoint(GeolocationValues.Count + 1, "0", "0", "0"));
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
            for (int i = 0; i < GeolocationValues.Count; i++)
            {
                GeolocationValues[i].Index = i + 1;
            }
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

            if (await TryParseSaveFeature())
            {
                await navigationService.NavigateBack(true);
                _isBusy = false;
                return;
            }

            _isBusy = false;
        }

        private async Task<bool> TryParseSaveFeature()
        {

            Feature feature = new Feature
            {
                type = "Feature",
                properties = new Properties()
            };



            // Feature type (Point, Line, Polygon).
            feature.geometry = new Geometry
            {
                type = thisEntryType
            };

            feature.properties.xamarincoordinates = new List<Point>();

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

                        feature.properties.typeIconPath = "point_icon.png";

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

                        feature.properties.typeIconPath = "line_icon.png";

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

                        
                        feature.properties.typeIconPath = "area_icon.png";

                        // This specific method of structuring points means that users will not
                        // be able to create multiple shapes in one polygon (whereas true GEOJSON allows that).
                        // This doesn't matter since our app interface can't allow for it anyway.
                        feature.geometry.coordinates = new List<object>(GeolocationValues.Count);
                        List<object> innerPoints = new List<object>(GeolocationValues.Count);

                        List<DisplayPoint> ClosedPoly = new List<DisplayPoint>(GeolocationValues);
                        ClosedPoly.Add(new DisplayPoint(GeolocationValues.Count + 1, GeolocationValues[0].Latitude, GeolocationValues[0].Longitude, GeolocationValues[0].Altitude));


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
            catch (Exception e)
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





            // A new entry will have an ID of NEW_ENTRY_ID as assigned from the constructor,
            // otherwise an ID will already be set for editing entries.
            feature.properties.id = thisEntryID;
            feature.properties.author = Preferences.Get("UserID", "Groundsman");
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


            bool success = false;
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
