using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Drawing;
using UIKit;
using Groundsman.iOS.Renderers;
using CoreGraphics;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace Groundsman.iOS.Renderers
{
    public class CustomEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            //Make sure element exists
            if (Element == null)
            {
                return;
            }

            //remove border
            Control.LeftView = new UIView(new CGRect(0, 0, 8, Control.Frame.Height));
            Control.RightView = new UIView(new CGRect(0, 0, 8, Control.Frame.Height));
            Control.LeftViewMode = UITextFieldViewMode.Always;
            Control.RightViewMode = UITextFieldViewMode.Always;
            Control.BorderStyle = UITextBorderStyle.None;
            Element.HeightRequest = 30;

            // Check for only Numeric keyboard
            if (this.Element.Keyboard == Keyboard.Numeric)
            {
                this.AddNumericToolbar();
            }
        }

        /// <summary>
        /// Add toolbar with Done and Negative button
        /// </summary>
        protected void AddNumericToolbar()
        {
            UIToolbar toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f))
            {
                Translucent = true,
            };
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
            {
                Control.ResignFirstResponder();
            });
            var negativeButton = new UIBarButtonItem("-", UIBarButtonItemStyle.Plain, delegate
            {
                Control.InsertText("-");
            });
            toolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FixedSpace),
                negativeButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                doneButton
                };
            Control.InputAccessoryView = toolbar;
        }
    }
}
