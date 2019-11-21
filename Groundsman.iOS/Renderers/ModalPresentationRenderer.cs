using Groundsman.iOS.Renderers;
using Groundsman;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(WelcomeFormView), typeof(ModalPresentationRenderer))]
namespace Groundsman.iOS.Renderers
{
    public class ModalPresentationRenderer : PageRenderer
    {
        public override void WillMoveToParentViewController(UIViewController parent)
        {
            parent.ModalInPresentation |= UIDevice.CurrentDevice.CheckSystemVersion(12, 0);
            base.WillMoveToParentViewController(parent);
        }
    }
}
