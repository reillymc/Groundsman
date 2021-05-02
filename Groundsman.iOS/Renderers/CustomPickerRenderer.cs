using CoreGraphics;
using Groundsman.iOS.Renderers;
using Groundsman.Misc;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomPicker), typeof(CustomPickerRenderer))]
namespace Groundsman.iOS.Renderers

{
    public class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            //Make sure element exists
            if (Element == null)
            {
                return;
            }

            //remove border
            Control.LeftView = new UIView(new CGRect(0, 0, 0, Control.Frame.Height));
            Control.RightView = new UIView(new CGRect(0, 0, 0, Control.Frame.Height));
            Control.LeftViewMode = UITextFieldViewMode.Always;
            Control.RightViewMode = UITextFieldViewMode.Always;
            Control.BorderStyle = UITextBorderStyle.None;
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                Control.Layer.CornerRadius = 8;
                Control.BackgroundColor = UIColor.FromName("customControlColor");
                Element.HeightRequest = 32;
            }
        }
    }
}