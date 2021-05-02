using Groundsman.iOS.Renderers;
using Groundsman.Views;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(WelcomeView), typeof(WelcomeModalRenderer))]
namespace Groundsman.iOS.Renderers
{
    public class WelcomeModalRenderer : PageRenderer
    {
        public override void WillMoveToParentViewController(UIViewController parent)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                parent.ModalInPresentation |= UIDevice.CurrentDevice.CheckSystemVersion(12, 0);
                base.WillMoveToParentViewController(parent);
            }
        }
    }
}