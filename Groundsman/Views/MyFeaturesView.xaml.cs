using Xamarin.Forms;
using Groundsman.ViewModels;
using System.Collections.Generic;

namespace Groundsman
{
    public partial class MyFeaturesView : ContentPage
    {
        MyFeaturesViewModel viewModel;
        List<SwipeView> swipeViews { set; get; }
        public MyFeaturesView()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyFeaturesViewModel();
            swipeViews = new List<SwipeView>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.GetFeatures();
        }

        private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
        {

            if (swipeViews.Count == 1)
            {
                swipeViews[0].Close();
                swipeViews.Remove(swipeViews[0]);
            }
        }

        private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
        {
            swipeViews.Add(sender as SwipeView);
        }
    }
}