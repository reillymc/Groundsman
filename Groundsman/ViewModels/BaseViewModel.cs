using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Groundsman.Interfaces;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    public IDataService<Feature> FeatureStore => DependencyService.Get<IDataService<Feature>>();
    public INavigationService<Feature> NavigationService => DependencyService.Get<INavigationService<Feature>>();
    public static ShakeService shakeService = App.shakeService;

    public ObservableCollection<Feature> FeatureList { get; set; }

    private bool isBusy = false;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    private string title = string.Empty;
    public string Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    public BaseViewModel()
    {
        FeatureList = (ObservableCollection<Feature>)FeatureStore.FeatureList;
    }

    public virtual async Task ShareFeature(Feature feature, View element = null)
    {
        System.Drawing.Rectangle bounds = System.Drawing.Rectangle.Empty;
        if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            if (element != null) bounds = element.GetAbsoluteBounds().ToSystemRectangle();
        }
        var share = new ShareFileRequest
        {
            Title = "Share Feature",
            File = new ShareFile(await FeatureHelper.ExportFeatures(feature), "application/json"),
            PresentationSourceBounds = bounds
        };
        await Share.RequestAsync(share);
    }

    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    public async Task OnDismiss(bool modal) => await NavigationService.NavigateBack(modal);

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChangedEventHandler changed = PropertyChanged;
        if (changed == null)
            return;

        changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
