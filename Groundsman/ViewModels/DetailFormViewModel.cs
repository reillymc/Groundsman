using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace Groundsman
{
    /// <summary>
    /// View-model for the page that shows a data entry's details in a form.
    /// </summary>
    public class DetailFormViewModel : ViewModelBase
    {

        public ICommand GetFeatureCommand { get; set; }
        public ICommand AddPointCommand { get; set; }
        public ICommand DeletePointCommand { get; set; }
        public ICommand AddMetadataFieldCommand { get; set; }
        public ICommand DeleteMetadataFieldCommand { get; set; }
        public ICommand OnSaveUpdatedCommand { get; set; }
        public ICommand DeleteEntryCommand { get; set; }
        public ICommand ClosePolyCommand { get; set; }

        // Property binding to determine if the delete button for metadata fields is visible, which is based on the type of this entry.
        public bool ShowPointDeleteBtn { get { return _numPointFields > minPoints; } }
        private int minPoints;

        // A reference to this entry's ID.
        private int thisEntryID;

        // A reference to this entry's type of structure.
        private string thisEntryType;

        private bool _isBusy;

        public ObservableCollection<Point> GeolocationPoints { get; set; }

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

        private float _metadataFloatEntry;
        public float MetadataFloatEntry
        {
            get { return _metadataFloatEntry; }
            set
            {
                _metadataFloatEntry = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// View-model constructor for adding new entries.
        /// </summary>
        public DetailFormViewModel(string entryType)
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
                    case "Line":
                        minPoints = 2;
                        break;
                    case "Polygon":
                        minPoints = 4;
                        break;
                    default:
                        break;
                }

                GeolocationPoints = new ObservableCollection<Point>();
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
        public DetailFormViewModel(Feature data)
        {


            thisEntryType = data.Geometry.Type;
            thisEntryID = data.Properties.Id;
            switch (thisEntryType)
            {
                case "Point":
                    minPoints = 1;
                    break;
                case "Line":
                    minPoints = 2;
                    break;
                case "Polygon":
                    minPoints = 4;
                    break;
            }

            NameEntry = data.Properties.Name;
            DateEntry = DateTime.Parse(data.Properties.Date).ToShortDateString();

            GeolocationPoints = new ObservableCollection<Point>(data.Properties.Xamarincoordinates);
            GeolocationEntryEnabled = true;

            MetadataStringEntry = data.Properties.MetadataStringValue;
            MetadataIntegerEntry = data.Properties.MetadataIntegerValue;
            MetadataFloatEntry = data.Properties.MetadataFloatValue;

            LoadingIconActive = false;
            NumPointFields = data.Properties.Xamarincoordinates.Count;
            InitCommandBindings();
        }

        /// <summary>
        /// Initialise command bindings.
        /// </summary>
        private void InitCommandBindings()
        {
            GetFeatureCommand = new Command<Point>(async (point) => { await GetGeoLocation(point); });

            AddPointCommand = new Command(() => AddPoint());
            DeletePointCommand = new Command<Point>((item) => DeletePoint(item));

            DeleteEntryCommand = new Command(async () => await DeleteFeatureEntry());

            OnSaveUpdatedCommand = new Command(async () => await OnSaveUpdateActivated());

            ClosePolyCommand = new Command(() => ClosePoly());
        }

        /// <summary>
        /// Queries the current device's location coordinates
        /// </summary>
        /// <param name="point">Point to set GPS data to.</param>
        private async Task GetGeoLocation(Point point)
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                // Disable interaction with entries to prevent errors.
                GeolocationEntryEnabled = false;
                LoadingIconActive = true;

                if (status == PermissionStatus.Granted)
                {
                    // Gets current location of device (MORE ACCURATE, but slower)
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                    var location = await Geolocation.GetLocationAsync(request);

                    // Re-enable interaction.
                    LoadingIconActive = false;
                    GeolocationEntryEnabled = true;

                    if (location != null)
                    {
                        point.Latitude = location.Latitude;
                        point.Longitude = location.Longitude;
                        point.Altitude = location.Altitude ?? 0.0;
                    }
                }
                else
                {
                    await HomePage.Instance.DisplayAlert("Location Permissions Required", "Enable location permissions for Groundsman in settings to use this feature.", "Ok");
                    // Re-enable interaction.
                    LoadingIconActive = false;
                    GeolocationEntryEnabled = true;
                }
            }
            catch (Exception)
            {
                await HomePage.Instance.DisplayAlert("Location Permissions Error", "Enable location permissions for Groundsman in settings to use this feature.", "Ok");
                point.Latitude = point.Longitude = point.Altitude = 0.0;
                // Re-enable interaction.
                LoadingIconActive = false;
                GeolocationEntryEnabled = true;
                //throw ex;
            }
        }

        /// <summary>
        /// Adds a new geolocation point to the list for line or polygon data types.
        /// </summary>
        /// <returns></returns>
        private void AddPoint()
        {
            if (_isBusy) return;
            _isBusy = true;

            GeolocationPoints.Add(new Point(0, 0, 0));
            NumPointFields++;
            _isBusy = false;
        }


        /// <summary>
        /// Deletes a geolocation point from the list.
        /// </summary>
        /// <param name="item">Item to delete</param>
        private void DeletePoint(Point item)
        {
            if (_isBusy) return;
            _isBusy = true;

            GeolocationPoints.Remove(item);
            NumPointFields--;
            _isBusy = false;
        }

        /// <summary>
        /// Deletes the entire feature from the list of current features and saves changes to the embedded file.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteFeatureEntry()
        {
            if (_isBusy) return;
            _isBusy = true;

            bool yesResponse = await HomePage.Instance.DisplayAlert("Delete Feature", "Are you sure you want to delete this feature?", "Yes", "No");
            if (yesResponse)
            {
                App.FeatureStore.DeleteFeatureAsync(thisEntryID);
                await HomePage.Instance.Navigation.PopToRootAsync();
            }

            _isBusy = false;
        }

        /// <summary>
        /// Saves a new or edited feature to the embedded file.
        /// </summary>
        async Task OnSaveUpdateActivated()
        {
            if (_isBusy) return;
            _isBusy = true;

            // Ensure geolocation points are only accurate up to the specified digit precision.
            foreach (var point in GeolocationPoints)
            {
                AppConstants.RoundGPSPosition(point);
            }

            // Do validation checks here.
            if (await FeatureEntryIsValid() == false)
            {
                _isBusy = false;
                return;
            }

            Feature featureToSave = CreateFeatureFromInput();

            App.FeatureStore.SaveFeatureAsync(featureToSave);
            await HomePage.Instance.Navigation.PopToRootAsync();

            _isBusy = false;
        }

        /// <summary>
        /// Creates a feature object based on the view-model data of this feature entry.
        /// </summary>
        /// <returns>A feature object formed from input values</returns>
        private Feature CreateFeatureFromInput()
        {
            Feature feature = new Feature();

            feature.Type = "Feature";
            feature.Properties = new Properties();

            // A new entry will have an ID of NEW_ENTRY_ID as assigned from the constructor,
            // otherwise an ID will already be set for editing entries.
            feature.Properties.Id = thisEntryID;

            if (Application.Current.Properties.ContainsKey("UserID"))
            {
                feature.Properties.AuthorId = Application.Current.Properties["UserID"] as string;
            }
            else
            {
                feature.Properties.AuthorId = string.Empty;
            }

            // Name and date of the feature.
            feature.Properties.Name = NameEntry;
            feature.Properties.Date = DateTime.Parse(DateEntry).ToShortDateString();

            // Metadata fields.
            feature.Properties.MetadataStringValue = MetadataStringEntry;
            feature.Properties.MetadataIntegerValue = MetadataIntegerEntry;
            feature.Properties.MetadataFloatValue = MetadataFloatEntry;

            // Feature type (Point, Line, Polygon).
            feature.Geometry = new Geometry();
            feature.Geometry.Type = thisEntryType;

            // Converts our xamarin coordinate data back into a valid geojson structure.
            {
                switch (thisEntryType)
                {
                    case "Point":
                        feature.Geometry.Coordinates = new List<object>() {
                        GeolocationPoints[0].Longitude,
                        GeolocationPoints[0].Latitude,
                        GeolocationPoints[0].Altitude };
                        break;
                    case "Line":
                        feature.Geometry.Coordinates = new List<object>(GeolocationPoints.Count);
                        for (int i = 0; i < GeolocationPoints.Count; i++)
                        {
                            feature.Geometry.Coordinates.Add(new JArray(new double[3] {
                            GeolocationPoints[i].Longitude,
                            GeolocationPoints[i].Latitude,
                            GeolocationPoints[i].Altitude }));
                        }

                        break;
                    case "Polygon":
                        // This specific method of structuring points means that users will not
                        // be able to create multiple shapes in one polygon (whereas true GEOJSON allows that).
                        // This doesn't matter since our app interface can't allow for it anyway.
                        feature.Geometry.Coordinates = new List<object>(GeolocationPoints.Count);
                        List<object> innerPoints = new List<object>(GeolocationPoints.Count);
                        for (int i = 0; i < GeolocationPoints.Count; i++)
                        {
                            innerPoints.Add(new JArray(new double[3] {
                            GeolocationPoints[i].Longitude,
                            GeolocationPoints[i].Latitude,
                            GeolocationPoints[i].Altitude }));
                        }
                        feature.Geometry.Coordinates.Add(innerPoints);
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
            if (string.IsNullOrEmpty(NameEntry))
            {
                await HomePage.Instance.DisplayAlert("Incomplete Entry", "Feature name must not be empty.", "OK");
                return false;
            }

            switch (thisEntryType)
            {
                case "Polygon":
                    if (GeolocationPoints.Count < 4)
                    {
                        await HomePage.Instance.DisplayAlert("Incomplete Entry", "A polygon must contain at least 4 data points.", "OK");
                        return false;
                    }

                    // Check if first and last points of the polygon have the same lat/long values.
                    {
                        double firstLatitude = GeolocationPoints[0].Latitude;
                        double lastLatitude = GeolocationPoints[GeolocationPoints.Count - 1].Latitude;
                        double firstLongitude = GeolocationPoints[0].Longitude;
                        double lastLongitude = GeolocationPoints[GeolocationPoints.Count - 1].Longitude;

                        if (firstLatitude != lastLatitude || firstLongitude != lastLongitude)
                        {
                            await HomePage.Instance.DisplayAlert("Incomplete Entry", "The first and last points of a polygon must match.", "OK");
                            return false;
                        }
                    }

                    break;
                case "Line":
                    if (GeolocationPoints.Count < 2)
                    {
                        await HomePage.Instance.DisplayAlert("Incomplete Entry", "A line must contain at least 2 data points.", "OK");
                        return false;
                    }

                    break;
                case "Point":
                    if (GeolocationPoints.Count != 1)
                    {
                        await HomePage.Instance.DisplayAlert("Impossible Entry", "A point must only contain 1 data point.", "OK");
                        return false;
                    }

                    break;
            }

            return true;
        }

        private void ClosePoly()
        {
            double latFist = GeolocationPoints[0].Latitude;
            double lonFist = GeolocationPoints[0].Longitude;
            double altFist = GeolocationPoints[0].Altitude;

            if (_isBusy) return;
            _isBusy = true;

            GeolocationPoints.Add(new Point(latFist, lonFist, altFist));
            NumPointFields++;

            _isBusy = false;
        }

    }
}
