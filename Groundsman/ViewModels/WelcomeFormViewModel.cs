using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public class WelcomeFormViewModel : BaseViewModel
    {
        private bool modal;

        public ICommand IDSubmitCommand { get; set; }

        private string _IDEntry;
        public string IDEntry
        {
            get { return _IDEntry; }
            set
            {
                _IDEntry = value;
                OnPropertyChanged();
            }
        }

        public WelcomeFormViewModel(bool modal)
        {
            this.modal = modal;
            IDSubmitCommand = new Command(async () => await SubmitIDEntry());
        }

        /// <summary>
        /// Submits the inputted ID entry from the user. If valid, the ID will be saved and the user continues to the main page.
        /// </summary>
        private async Task SubmitIDEntry()
        {
            if (string.IsNullOrWhiteSpace(IDEntry) == false)
            {
                Preferences.Set("UserID", IDEntry);
                await Application.Current.SavePropertiesAsync();
                await navigationService.NavigateBack(modal);
            }
            else
            {
                await navigationService.ShowAlert("Invalid ID", "Your user ID cannot be empty.", false);
            }
        }
    }
}
