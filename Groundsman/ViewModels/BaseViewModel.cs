using Groundsman.Interfaces;
using Groundsman.Models;
using Groundsman.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Groundsman.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public IDataStore<Feature> FeatureStore => DependencyService.Get<IDataStore<Feature>>();
        public INavigationService<Feature> NavigationService => DependencyService.Get<INavigationService<Feature>>();
        public static LogStore LogStore = new LogStore();

        private ObservableRangeCollection<Feature> featureList = new ObservableRangeCollection<Feature>();
        public ObservableRangeCollection<Feature> FeatureList
        {
            get { return featureList; }
            set
            {
                if (featureList == value)
                    return;
                featureList = value;
            }
        }

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
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

        public async Task OnDismiss(bool modal)
        {
            await NavigationService.NavigateBack(modal);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
