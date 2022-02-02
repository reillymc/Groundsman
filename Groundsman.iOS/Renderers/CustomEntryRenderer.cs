using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using CoreGraphics;
using Groundsman.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace Groundsman.iOS.Renderers;

public class CustomEntryRenderer : EntryRenderer
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

    protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
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

        // Check for only Numeric keyboard
        if (Element.Keyboard == Keyboard.Numeric)
        {
            AddNumericToolbar();
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
        UIBarButtonItem doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
        {
            Control.ResignFirstResponder();
        });
        UIBarButtonItem negativeButton = new UIBarButtonItem("-", UIBarButtonItemStyle.Plain, delegate
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
