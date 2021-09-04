using System;
using Groundsman.Models;
using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{
    public partial class FeatureListView : ContentPage
    {
        private readonly FeatureListViewModel viewModel;
        private ViewCell lastCell;

        public FeatureListView()
        {
            InitializeComponent();
            BindingContext = viewModel = new FeatureListViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //_ = viewModel.LoadFeatures();
        }


        public void DeselectItem(object sender, EventArgs e)
        {
            object selected = ((ListView)sender).SelectedItem;
            if (selected != null)
            {
                viewModel.ItemTappedCommand.Execute((Feature)selected);
                ((ListView)sender).SelectedItem = null;
            }
        }

        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                if (lastCell != null)
                    lastCell.View.BackgroundColor = Color.Default;
                ViewCell viewCell = (ViewCell)sender;
                if (viewCell.View != null)
                {
                    viewCell.View.BackgroundColor = App.AppTheme == App.Theme.Light ? Color.White : Color.FromHex("#111111");
                    lastCell = viewCell;
                }
            }
        }
    }
}
