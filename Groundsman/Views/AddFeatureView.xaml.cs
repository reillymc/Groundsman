using Groundsman.Services;
using Groundsman.ViewModels;
using System;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class AddFeatureView : ContentPage
    {
        AddFeatureViewModel viewModel;
        bool modal;

        public AddFeatureView(bool isModal)
        {
            InitializeComponent();
            BindingContext = viewModel = new AddFeatureViewModel(isModal);
            modal = isModal;
        }

        async void OnDismissButtonClicked(object sender, EventArgs args)
        {
            NavigationService navigationService = new NavigationService();
            await navigationService.NavigateBack(modal);
        }
    }
}
