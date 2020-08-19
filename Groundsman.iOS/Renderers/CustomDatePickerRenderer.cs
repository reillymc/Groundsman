using CoreGraphics;
using Groundsman.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(CustomDatePickerRenderer))]
namespace Groundsman.iOS.Renderers

{
    public class CustomDatePickerRenderer : DatePickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            //Make sure element exists
            if (Element == null)
            {
                return;
            }

            //remove border
            this.Control.LeftView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
            this.Control.RightView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
            this.Control.LeftViewMode = UITextFieldViewMode.Always;
            this.Control.RightViewMode = UITextFieldViewMode.Always;
            this.Control.BorderStyle = UITextBorderStyle.None;
            this.Element.HeightRequest = 30;
        }
    }
}