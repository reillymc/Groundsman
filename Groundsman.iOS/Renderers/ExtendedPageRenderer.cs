using Groundsman.iOS.Renderers;
using Groundsman.Styles;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(ExtendedPageRenderer))]
namespace Groundsman.iOS.Renderers
{
    public class ExtendedPageRenderer : PageRenderer
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (NavigationController != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                    NavigationController.ExtendedLayoutIncludesOpaqueBars = true;
                    NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
                    NavigationController.NavigationBar.SizeToFit();
                }
            }
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            try
            {
                SetAppTheme();
            }
            catch { }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                if (TraitCollection.UserInterfaceStyle != previousTraitCollection.UserInterfaceStyle)
                {
                    SetAppTheme();
                }
            }
        }

        private void SetAppTheme()
        {
            if (TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark)
            {
                if (App.AppTheme == App.Theme.Dark)
                    return;

                Xamarin.Forms.Application.Current.Resources = new DarkTheme();

                App.AppTheme = App.Theme.Dark;
            }
            else
            {
                if (App.AppTheme != App.Theme.Dark)
                    return;
                Xamarin.Forms.Application.Current.Resources = new LightTheme();
                App.AppTheme = App.Theme.Light;
            }
        }
    }

}
