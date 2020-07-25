using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Drawing;
using UIKit;
using Groundsman.iOS.Renderers;
using CoreGraphics;
using System.ComponentModel;

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
            this.Control.LeftView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
            this.Control.RightView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
            this.Control.LeftViewMode = UITextFieldViewMode.Always;
            this.Control.RightViewMode = UITextFieldViewMode.Always;
            this.Control.BorderStyle = UITextBorderStyle.None;
            this.Element.HeightRequest = 30;

            // Check for only Numeric keyboard
            if (this.Element.Keyboard == Keyboard.Numeric)
            {
                this.AddNegDoneButton();
            }
        }

        /// <summary>
        /// Add toolbar with Done button
        /// </summary>

        protected void AddDoneButton()
        {
            UIToolbar toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f));
            toolbar.Translucent = true;

            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
            {
                this.Control.ResignFirstResponder();
            });

            toolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                doneButton

                };
            this.Control.InputAccessoryView = toolbar;
        }

        protected void AddNegDoneButton()
        {
            UIToolbar toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f));
            toolbar.Translucent = true;

            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
            {
                this.Control.ResignFirstResponder();
            });
            var negButton = new UIBarButtonItem("-", UIBarButtonItemStyle.Plain, delegate
            {
                this.Control.InsertText("-");
            });

            toolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FixedSpace),
                negButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                doneButton

                };
            this.Control.InputAccessoryView = toolbar;
        }
    }
}
