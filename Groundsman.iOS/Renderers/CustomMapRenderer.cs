using Xamarin.Forms;
using Groundsman;
using Groundsman.iOS.Renderers;
using MapKit;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Maps.iOS;
using CoreGraphics;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace Groundsman.iOS.Renderers
{
    public class CustomMapRenderer : MapRenderer
    {
        readonly UIColor GroundsmanGreen = new UIColor(red: 0.30f, green: 0.69f, blue: 0.31f, alpha: 1.00f);
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null && e.NewElement != null)
            {
                MKMapView nativeMap = Control as MKMapView;
                nativeMap.ShowsCompass = false;
                nativeMap.TintColor = GroundsmanGreen;

                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    AddToolbar(nativeMap);
                }
                else
                {
                    AddTrackingButton(nativeMap);
                }

                AddCompassButton(nativeMap);
            }
        }

        private void AddTrackingButton(MKMapView nativeMap)
        {
            MKUserTrackingButton button = MKUserTrackingButton.FromMapView(nativeMap);
            button.Layer.BackgroundColor = UIColor.White.ColorWithAlpha(0.9F).CGColor;
            button.Layer.BorderColor = UIColor.White.ColorWithAlpha(0.9F).CGColor;
            button.Layer.ShadowColor = UIColor.Black.CGColor;
            button.Layer.ShadowOpacity = 0.3F;
            button.Layer.ShadowRadius = 4;
            button.Layer.BorderWidth = 1;
            button.Layer.CornerRadius = 4;
            button.Layer.MasksToBounds = false;
            button.TranslatesAutoresizingMaskIntoConstraints = false;

            nativeMap.AddSubview(button);
            button.TopAnchor.ConstraintEqualTo(nativeMap.TopAnchor, 58).Active = true;
            button.RightAnchor.ConstraintEqualTo(nativeMap.RightAnchor, -12).Active = true;
        }

        private void AddCompassButton(MKMapView nativeMap)
        {
            MKCompassButton compass = MKCompassButton.FromMapView(nativeMap);
            compass.CompassVisibility = MKFeatureVisibility.Visible;
            compass.Frame = new CGRect(new CGPoint(45, 45), compass.Frame.Size);
            compass.TranslatesAutoresizingMaskIntoConstraints = false;

            nativeMap.AddSubview(compass);

            compass.TopAnchor.ConstraintEqualTo(nativeMap.TopAnchor, 108).Active = true;
            compass.RightAnchor.ConstraintEqualTo(nativeMap.RightAnchor, -14).Active = true;
        }

        private void AddToolbar(MKMapView nativeMap)
        {
            // Tracking button
            MKUserTrackingBarButtonItem trackingButton = new MKUserTrackingBarButtonItem(nativeMap);
            trackingButton.CustomView.TintColor = GroundsmanGreen;
            trackingButton.CustomView.Frame = new CGRect(x: trackingButton.CustomView.Frame.X, y: trackingButton.CustomView.Frame.Y, width: 44, height: 44);

            // Toolbar and frame
            var toolBarFrame = new CGRect(location: new CGPoint(x: 0, y: 0), size: new CGSize(width: 44, height: 44));
            var toolbar = new UIToolbar(frame: toolBarFrame)
            {
                BarTintColor = UIColor.SystemBackgroundColor,
                Translucent = true
            };

            UIBarButtonItem flex = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, target: this, action: null);

            // Set toolbar items - can be expanded later
            toolbar.SetItems(new UIBarButtonItem[] { flex, trackingButton, flex }, true);
            // Set toolbar position
            var origin = new CGPoint(x: UIScreen.MainScreen.Bounds.Width - 53, y: 52);

            // Create base view for button
            UIView baseView = new UIView(frame: new CGRect(location: origin, size: new CGSize(width: 44, height: 44)))
            {
                BackgroundColor = UIColor.Clear
            };

            // Shadow effects
            baseView.Layer.ShadowColor = UIColor.Black.CGColor;
            baseView.Layer.ShadowOpacity = 0.3f;
            baseView.Layer.ShadowOffset = new CGSize(width: 0, height: 0);
            baseView.Layer.ShadowRadius = 4.0f;

            // Improves performance but causes flickering and artefacts
            //baseView.Layer.ShouldRasterize = true;
            //baseView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;

            // Rounded border view for button
            UIView borderview = new UIView
            {
                Frame = baseView.Bounds
            };
            borderview.Layer.CornerRadius = 10;
            borderview.Layer.MasksToBounds = true;

            // Stack subviews
            baseView.AddSubview(borderview);
            borderview.AddSubview(toolbar);
            nativeMap.AddSubview(baseView);
        }
    }
}
