using System;
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
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null && e.NewElement != null)
            {
                MKMapView nativeMap = Control as MKMapView;
                nativeMap.ShowsCompass = false;
                AddUserTrackingButton(nativeMap);

                
            }
        }

        private void AddUserTrackingButton(MKMapView nativeMap)
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

            MKCompassButton compass = MKCompassButton.FromMapView(nativeMap);
            compass.CompassVisibility = MKFeatureVisibility.Visible;
            compass.Frame = new CGRect(new CGPoint(45 , 45), compass.Frame.Size);
            compass.TranslatesAutoresizingMaskIntoConstraints = false;

            nativeMap.AddSubview(button);
            nativeMap.AddSubview(compass);

            button.TopAnchor.ConstraintEqualTo(nativeMap.TopAnchor, 50).Active = true;
            button.RightAnchor.ConstraintEqualTo(nativeMap.RightAnchor, -4).Active = true;

            compass.TopAnchor.ConstraintEqualTo(nativeMap.TopAnchor, 100).Active = true;
            compass.RightAnchor.ConstraintEqualTo(nativeMap.RightAnchor, -6).Active = true;
        }
    }
}
