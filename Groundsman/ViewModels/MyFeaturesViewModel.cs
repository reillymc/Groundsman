using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    /// <summary>
    /// View-model for the page that shows the list of data entries.
    /// </summary>
    public class MyFeaturesViewModel : BaseViewModel
    {
        public Command AddButtonTappedCommand { set; get; }
        public Command ShareButtonTappedCommand { set; get; }
        public Command ItemTappedCommand { set; get; }
        public Command EditEntryCommand { get; set; }
        public Command DeleteEntryCommand { get; set; }

        private bool _isBusy;

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

            GetFeatures();
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

            await navigationService.NavigateToDetailPage(data);

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

            await navigationService.InvokeShareSheetAsync();

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

            await navigationService.PushAddFeaturePage();

            _isBusy = false;
        }

        /// <summary>
        /// Displays the edit page for the selected feature.
        /// </summary>
        /// <param name="feature">Feature to edit.</param>
        private async Task ShowEditFeatureDetailsPage(Feature feature)
        {
            Debug.WriteLine("HIIII");
            if (_isBusy) return;
            _isBusy = true;

            await navigationService.NavigateToEditPage(feature);

            _isBusy = false;
        }

        /// <summary>
        /// Call the Feature Store to delete seleted feature.
        /// </summary>
        /// <param name="feature">Feature to delete.</param>
        /// <returns></returns>
        private async Task DeleteFeature(Feature feature)
        {
            Debug.WriteLine("HIIII");
            if (_isBusy) return;
            _isBusy = true;

            bool yesResponse = await HomePage.Instance.DisplayAlert("Delete Feature", "Are you sure you want to delete this feature?", "Yes", "No");
            if (yesResponse)
            {
                await featureStore.DeleteItemAsync(feature);
            }

            _isBusy = false;
        }

        /// <summary>
        /// Call the feature store to fetch from file and then set the resulting current features to the list source collection.
        /// </summary>
        public async void GetFeatures()
        {
            ObservableCollection<Feature> updates = await featureStore.GetItemsAsync();
            FeatureList.ReplaceRange(updates);
        }
    }
}