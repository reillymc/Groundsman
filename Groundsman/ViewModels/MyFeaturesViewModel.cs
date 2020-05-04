using System.ComponentModel;
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
        public ICommand AddButtonTappedCommand { set; get; }
        public ICommand ShareButtonTappedCommand { set; get; }
        public ICommand ItemTappedCommand { set; get; }
        public ICommand EditEntryCommand { get; set; }
        public ICommand DeleteEntryCommand { get; set; }

        RootObject featureList = new RootObject();
        public RootObject FeatureList
        {
            get { return featureList; }
            set
            {
                if (featureList == value)
                    return;
                featureList = value;
                //PropertyChanged
            }
        }

        private bool _isBusy;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// View-model constructor.
        /// </summary>
        public MyFeaturesViewModel()
        {
            AddButtonTappedCommand = new Command(async () => await AddButtonTapped());
            ShareButtonTappedCommand = new Command(async () => await ShowShareSheet());
            ItemTappedCommand = new Command<Feature>(async (data) => await ShowFeatureDetailsPage(data));
            EditEntryCommand = new Command<Feature>(async (feature) => await ShowEditFeatureDetailsPage(feature));
            DeleteEntryCommand = new Command<Feature>(async (feature) => await DeleteFeature(feature));

            _ = GetFeatures();
        }

        /// <summary>
        /// Opens the ExistingDetailFormView page showing more detail about the feature the user tapped on in the list.
        /// </summary>
        /// <param name="data">Feature tapped on to be displayed.</param>
        /// <returns></returns>
        private async Task ShowFeatureDetailsPage(Feature data)
        {
            if (_isBusy) return;
            _isBusy = true;

            await HomePage.Instance.ShowDetailFormPage(data);

            _isBusy = false;
        }

        /// <summary>
        /// Shows native share sheet that exports all features.
        /// </summary>
        /// <returns></returns>
        private async Task ShowShareSheet()
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
        private async Task AddButtonTapped()
        {
            if (_isBusy) return;
            _isBusy = true;

            await HomePage.Instance.ShowAddFeaturePage();

            _isBusy = false;
        }

        /// <summary>
        /// Displays the edit page for the selected feature.
        /// </summary>
        /// <param name="feature">Feature to edit.</param>
        private async Task ShowEditFeatureDetailsPage(Feature feature)
        {
            if (_isBusy) return;
            _isBusy = true;

            await HomePage.Instance.ShowEditDetailFormPage(feature);

            _isBusy = false;
        }

        /// <summary>
        /// Call the Feature Store to delete seleted feature.
        /// </summary>
        /// <param name="feature">Feature to delete.</param>
        /// <returns></returns>
        private async Task DeleteFeature(Feature feature)
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

        /// <summary>
        /// Call the feature store to fetch from file and then set the resulting current features to the list source collection.
        /// </summary>
        private async Task GetFeatures()
        {
            await App.FeatureStore.FetchFeaturesFromFile();
            FeatureList.features = App.FeatureStore.CurrentFeatures;
        }
    }
}