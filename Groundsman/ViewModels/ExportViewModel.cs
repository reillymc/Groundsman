using System.Windows.Input;
using Xamarin.Forms;

namespace Groundsman
{
    public class ExportViewModel : ViewModelBase
    {
        public ICommand ShareButtonClickCommand { set; get; }
        public ICommand CopyButtonClickCommand { set; get; }

        /// <summary>
        /// View-model constructor for the export page.
        /// </summary>
        public ExportViewModel()
        {
            ShareButtonClickCommand = new Command(async () =>
            {
                await App.FeatureStore.ExportFeatures();
            });

            CopyButtonClickCommand = new Command(async () =>
            {
                await App.FeatureStore.CopyFeaturesToClipboard();
            });
        }
    }
}
