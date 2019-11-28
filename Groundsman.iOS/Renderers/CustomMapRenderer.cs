using System;
using Xamarin.Forms;
using Groundsman;
using Groundsman.iOS.Renderers;
using MapKit;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Maps.iOS;

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
            nativeMap.AddSubview(button);
            button.TopAnchor.ConstraintEqualTo(nativeMap.TopAnchor, 90).Active = true;
            button.RightAnchor.ConstraintEqualTo(nativeMap.RightAnchor, -4).Active = true;
        }
    }
}
