using Groundsman.iOS.Renderers;
using Groundsman.Misc;
using Groundsman.Views;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(LoggerView), typeof(LoggerModalRenderer))]
namespace Groundsman.iOS.Renderers
{
    public class LoggerModalRenderer : PageRenderer
    {
        public override void WillMoveToParentViewController(UIViewController parent)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                HandleReceivedMessages(parent);
                base.WillMoveToParentViewController(parent);
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                Unsubscribe();
            }
            base.ViewDidAppear(animated);
        }

        /// <summary>
        /// Stops modal being dismissable when logger is active
        /// </summary>
        /// <param name="parent"></param>
        void HandleReceivedMessages(UIViewController parent)
        {
            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message => { parent.ModalInPresentation = false; });
            MessagingCenter.Subscribe<StartServiceMessage>(this, "ServiceStarted", message => { parent.ModalInPresentation = true; });
        }

        void Unsubscribe()
        {
            MessagingCenter.Unsubscribe<StopServiceMessage>(this, "ServiceStopped");
            MessagingCenter.Unsubscribe<StartServiceMessage>(this, "ServiceStarted");
        }
    }
}