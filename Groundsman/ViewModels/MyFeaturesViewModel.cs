using System.Threading.Tasks;
using Groundsman.Models;
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
        public Command DeleteEntryCommand { get; set; }

        /// <summary>
        /// View-model constructor.
        /// </summary>
        public MyFeaturesViewModel()
        {
            AddButtonTappedCommand = new Command(async () => await AddButtonTapped());
            ShareButtonTappedCommand = new Command(async () => await ShowShareSheet());
            ItemTappedCommand = new Command<Feature>(async (feature) => await ShowEditFeatureDetailsPage(feature));
            DeleteEntryCommand = new Command<Feature>(async (feature) => await DeleteFeature(feature));

            Title = "My Features";
        }

        /// <summary>
        /// Shows native share sheet that exports all features.
        /// </summary>
        /// <returns></returns>
        private async Task ShowShareSheet()
        {
            if (IsBusy) return;
            IsBusy = true;

            await FeatureStore.ExportFeatures(App.featureList);

            IsBusy = false;
        }

        /// <summary>
        /// Opens up the dialog box where the user can select between Point, Line, and Polygon feature types to add.
        /// </summary>
        /// <returns></returns>
        private async Task AddButtonTapped()
        {
            if (IsBusy) return;
            IsBusy = true;

            await NavigationService.PushAddFeaturePage();

            IsBusy = false;
        }

        /// <summary>
        /// Displays the edit page for the selected feature.
        /// </summary>
        /// <param name="feature">Feature to edit.</param>
        private async Task ShowEditFeatureDetailsPage(Feature feature)
        {
            if (IsBusy) return;
            IsBusy = true;

            await NavigationService.NavigateToEditPage(feature);

            IsBusy = false;
        }

        /// <summary>
        /// Call the Feature Store to delete seleted feature.
        /// </summary>
        /// <param name="feature">Feature to delete.</param>
        /// <returns></returns>
        private async Task DeleteFeature(Feature feature)
        {
            if (IsBusy) return;
            IsBusy = true;

            shakeService.Start();
            await FeatureStore.DeleteItemAsync(feature);

            IsBusy = false;
        }
    }
}
