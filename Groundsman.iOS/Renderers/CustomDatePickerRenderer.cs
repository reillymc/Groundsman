using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (this.Control != null)
            {
                Control.Started += Control_StartedAsync;
                Control.Ended += Control_Ended;
            }
        }

        private void Control_Ended(object sender, EventArgs e)
        {
            var s = Control.Superview;
            while (s != null)
            {
                if (s is UICollectionView)
                {
                    var ss = s as UICollectionView;
                    ss.ScrollEnabled = true;
                }
                s = s.Superview;
            }
        }

        private void Control_StartedAsync(object sender, EventArgs e)
        {
            var s = Control.Superview;
            while (s != null)
            {
                if (s is UICollectionView)
                {
                    var ss = s as UICollectionView;
                    if (ss.ShowsHorizontalScrollIndicator)
                    {
                        ss.ScrollEnabled = false;
                    }

                    BeginInvokeOnMainThread(async () =>
                    {
                        await Task.Delay(500);
                        ss.ScrollEnabled = true;
                    });

                }
                s = s.Superview;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
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
            Element.HeightRequest = 30;
        }
    }
}