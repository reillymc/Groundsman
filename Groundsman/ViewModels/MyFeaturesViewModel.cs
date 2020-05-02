using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Groundsman
{
    /// <summary>
    /// View-model for the page that shows the list of data entries.
    /// </summary>
    public class MyFeaturesViewModel : INotifyPropertyChanged
    {
        // Static flag that determines whether the features list should be updated or not.
        public static bool isDirty = true;

        public ICommand ButtonClickedCommand { set; get; }
        public ICommand IDClickedCommand { set; get; }
        public ICommand ItemTappedCommand { set; get; }
        public ICommand EditEntryCommand { get; set; }
        public ICommand DeleteEntryCommand { get; set; }

        private bool _isBusy;

        RootObject featureList = new RootObject();

        public RootObject FeatureList
        {
            get { return featureList; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (featureList == value)
                    return;
                featureList = value;
                OnPropertyChanged();
                Debug.WriteLine("Changed");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }









        /// <summary>
        /// View-model constructor.
        /// </summary>
        public MyFeaturesViewModel()
        {
            ButtonClickedCommand = new Command(async () => await ExecuteButtonClickedCommand());
            IDClickedCommand = new Command(async () => await IDTappedCommandAsync());
            ItemTappedCommand = new Command<Feature>(async (data) => await ExecuteItemTappedCommand(data));
            EditEntryCommand = new Command<Feature>((feature) => EditFeatureEntry(feature));
            DeleteEntryCommand = new Command<Feature>(async (feature) => await DeleteFeatureEntry(feature));

            GetFeatures();

        }

        /// <summary>
        /// Opens the ExistingDetailFormView page showing more detail about the feature the user tapped on in the list.
        /// </summary>
        /// <param name="data">Feature tapped on.</param>
        /// <returns></returns>
        private async Task ExecuteItemTappedCommand(Feature data)
        {
            if (_isBusy) return;
            _isBusy = true;

            await HomePage.Instance.ShowExistingDetailFormPage(data);

            _isBusy = false;
        }

        private async Task IDTappedCommandAsync()
        {
            if (_isBusy) return;
            _isBusy = true;

            await HomePage.Instance.ShowShareSheetAsync();

            _isBusy = false;
        }

        /// <summary>
        /// Opens up the dialog box where the user can select between Point, Line, and Polygon feature types to add.
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteButtonClickedCommand()
        {
            if (_isBusy) return;
            _isBusy = true;

            await HomePage.Instance.ShowDetailFormOptions();

            _isBusy = false;
        }

        private async void GetFeatures()
        {
            await App.FeatureStore.FetchFeaturesFromFile();
            FeatureList.features = App.FeatureStore.CurrentFeatures;
        }

        /// <summary>
        /// Displays the edit page for the selected feature.
        /// </summary>
        /// <param name="feature">Feature to edit.</param>
        private void EditFeatureEntry(Feature feature)
        {
            if (_isBusy) return;
            _isBusy = true;

            HomePage.Instance.ShowEditDetailFormPage(feature);

            _isBusy = false;
        }

        private async Task DeleteFeatureEntry(Feature feature)
        {
            if (_isBusy) return;
            _isBusy = true;

            bool yesResponse = await HomePage.Instance.DisplayAlert("Delete Feature", "Are you sure you want to delete this feature?", "Yes", "No");
            if (yesResponse)
            {
                App.FeatureStore.DeleteFeatureAsync(feature);
            }
            _isBusy = false;
        }
    }
}