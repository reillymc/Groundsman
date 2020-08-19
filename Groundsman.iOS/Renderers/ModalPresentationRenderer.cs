using Groundsman.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Groundsman.Views;

[assembly: ExportRenderer(typeof(WelcomeFormView), typeof(ModalPresentationRenderer))]
namespace Groundsman.iOS.Renderers
{
    public class ModalPresentationRenderer : PageRenderer
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