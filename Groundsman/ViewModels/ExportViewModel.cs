using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.IO;

namespace Groundsman
{
    public class ExportViewModel : ViewModelBase
    {
        public ICommand ShareButtonClickCommand { set; get; }

        public ICommand BackupButtonClickCommand { set; get; }
        private const string EMBEDDED_FILENAME = "locations.json";

        private string _EmailEntry;
        public string EmailEntry
        {
            get { return _EmailEntry; }
            set
            {
                _EmailEntry = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// View-model constructor for the export page.
        /// </summary>
        public ExportViewModel()
        {
            ExperimentalFeatures.Enable("ShareFileRequest_Experimental");
            ShareButtonClickCommand = new Command(async () =>
            {
                string featuresFile =  App.FeatureStore.GetEmbeddedFile();
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Features Export",
                    File = new ShareFile(Path.Combine(FileSystem.AppDataDirectory, EMBEDDED_FILENAME), "text/plain")
                });
            });

            BackupButtonClickCommand = new Command(async () =>
            {
                string textFile =  App.FeatureStore.GetEmbeddedFile();
                await Clipboard.SetTextAsync(textFile);

                await HomePage.Instance.DisplayAlert("Copy Features", "Features successfully copied to clipboard.", "OK");
            });
        }
    }
}
