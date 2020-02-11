using System.Windows.Input;
using Xamarin.Forms;

namespace Groundsman
{
    public class ImportViewModel : ViewModelBase
    {
        public ICommand ImportFileButtonClickCommand { set; get; }
        public ICommand ImportClipboardButtonClickCommand { set; get; }

        /// <summary>
        /// View-model constructor for the import page.
        /// </summary>
        public ImportViewModel()
        {
            ImportFileButtonClickCommand = new Command(async () =>
            {
                await App.FeatureStore.ImportFeaturesFromFile();
            });

            ImportClipboardButtonClickCommand = new Command(async () =>
            {
                await App.FeatureStore.ImportFeaturesFromClipboard();
            });
        }
    }
}
