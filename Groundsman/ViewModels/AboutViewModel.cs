using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels;

public class AboutViewModel : BaseViewModel
{
    public Command IDSubmitCommand { get; set; }

    public string CurrentVersion { get; set; } = "0.0";

    public AboutViewModel()
    {
        CurrentVersion = VersionTracking.CurrentVersion;
    }
}
