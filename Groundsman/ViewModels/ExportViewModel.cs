using Plugin.Share;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
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

            // If share button clicked
            if (!CrossShare.IsSupported)
                return;

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
