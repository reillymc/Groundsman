using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels;

public class WelcomeViewModel : BaseViewModel
{

    public Command IDSubmitCommand { get; set; }

    private string _IDEntry;
    public string IDEntry
    {
        get => _IDEntry;
        set
        {
            _IDEntry = value;
            IDSubmitCommand.ChangeCanExecute();
            OnPropertyChanged();
        }
    }

    public WelcomeViewModel() => IDSubmitCommand = new Command(async () => await SubmitIDEntry(), () => !string.IsNullOrWhiteSpace(IDEntry));

    /// <summary>
    /// Submits the inputted ID entry from the user. If valid, the ID will be saved and the user continues to the main page.
    /// </summary>
    private async Task SubmitIDEntry()
    {
        if (!string.IsNullOrWhiteSpace(IDEntry))
        {
            Preferences.Set(Constants.UserIDKey, IDEntry);
            await Application.Current.SavePropertiesAsync();
            await NavigationService.NavigateBack(true);
            Constants.FirstRun = false;
        }
    }
}
