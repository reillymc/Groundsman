using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Groundsman.Models;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public abstract class BaseEditFeatureViewModel : BaseViewModel
    {
        public ICommand OnDoneTappedCommand { get; set; }
        public ICommand OnCancelTappedCommand { get; set; }
        public ICommand ShareButtonClickCommand { get; set; }
        public ICommand GetFeatureCommand { get; set; }
        public ICommand DeleteFeatureCommand { get; set; }

        public readonly Feature Feature = new Feature { Type = GeoJSONType.Feature };
        public ObservableCollection<DisplayPosition> Positions { get; set; } = new ObservableCollection<DisplayPosition>();

        public bool IsExistingFeature { get; set; } = false;

        public string NameEntry { get; set; }
        public string DateEntry { get; set; }

        public BaseEditFeatureViewModel()
        {
            OnDoneTappedCommand = new Command(async () => await SaveDismiss());
            ShareButtonClickCommand = new Command<View>(async (view) => await ShareFeature(view));
            OnCancelTappedCommand = new Command(async () => await CancelDismiss());
            DeleteFeatureCommand = new Command(async () => await DeleteDismiss());
        }

        public abstract Task ShareFeature(View view);

        public abstract Task SaveDismiss();

        public abstract Task CancelDismiss();

        public abstract void AnyDismiss();
        public abstract Task DeleteDismiss();
    }
}
