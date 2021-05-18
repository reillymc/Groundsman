using System;
using CoreGraphics;
using Groundsman;
using Groundsman.Interfaces;
using Groundsman.iOS.Renderers;
using Groundsman.Models;
using Groundsman.Services;
using MapKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

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

                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    AddToolbar(nativeMap);
                    AddCompassButton(nativeMap, 152);
                }
                else
                {
                    AddTrackingButton(nativeMap);
                    AddCompassButton(nativeMap, 108);
                }
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

        private void AddCompassButton(MKMapView nativeMap, int topAnchor)
        {
            MKCompassButton compass = MKCompassButton.FromMapView(nativeMap);
            compass.CompassVisibility = MKFeatureVisibility.Visible;
            compass.Frame = new CGRect(new CGPoint(45, 45), compass.Frame.Size);
            compass.TranslatesAutoresizingMaskIntoConstraints = false;

            nativeMap.AddSubview(compass);

            compass.TopAnchor.ConstraintEqualTo(nativeMap.TopAnchor, topAnchor).Active = true;
            compass.RightAnchor.ConstraintEqualTo(nativeMap.RightAnchor, -14).Active = true;
        }

        private void AddToolbar(MKMapView nativeMap)
        {
            // Map Tracking button
            MKUserTrackingBarButtonItem trackingButton = new MKUserTrackingBarButtonItem(nativeMap);
            trackingButton.CustomView.Frame = new CGRect(x: trackingButton.CustomView.Frame.X, y: trackingButton.CustomView.Frame.Y, width: 44, height: 44);
            trackingButton.CustomView.Transform = CGAffineTransform.MakeRotation((nfloat)(-270.0 / 180 * Math.PI));

            // Add Feature Button
            UIButton addFeatureButton = new UIButton(UIButtonType.ContactAdd)
            {
                Frame = new CGRect(x: trackingButton.CustomView.Frame.X, y: trackingButton.CustomView.Frame.Y, width: 44, height: 44),
                Transform = CGAffineTransform.MakeRotation((nfloat)(-270.0 / 180 * Math.PI))
            };
            addFeatureButton.TouchUpInside += (sender, e) =>
            {
                NavigationService navigationService = (NavigationService)DependencyService.Get<INavigationService<Feature>>();
                _ = navigationService.PushAddFeaturePage();
            };
            UIBarButtonItem addFeatureButtonItem = new UIBarButtonItem(addFeatureButton);

            // Flexible Seperator
            UIBarButtonItem flex = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, target: this, action: null);

            // Toolbar Items Aray
            UIBarButtonItem[] ButtonsArray = new UIBarButtonItem[] { flex, trackingButton, flex, addFeatureButtonItem, flex };

            // Toolbar and frame
            CGRect toolBarFrame = new CGRect(location: new CGPoint(x: 0, y: 0), size: new CGSize(width: 88, height: 44));
            UIToolbar toolbar = new UIToolbar(frame: toolBarFrame)
            {
                BarTintColor = UIColor.SystemBackgroundColor,
                Translucent = true
            };
            toolbar.SetItems(ButtonsArray, true);

            // Set toolbar position
            CGPoint origin = new CGPoint(x: UIScreen.MainScreen.Bounds.Width - 75, y: 75);

            // Create base view for button
            UIView baseView = new UIView(frame: new CGRect(location: origin, size: new CGSize(width: 88, height: 44)))
            {
                BackgroundColor = UIColor.Clear
            };

            // Shadow effects
            baseView.Layer.ShadowColor = UIColor.Black.CGColor;
            baseView.Layer.ShadowOpacity = 0.3f;
            baseView.Layer.ShadowOffset = new CGSize(width: 0, height: 0);
            baseView.Layer.ShadowRadius = 4.0f;

            // Rounded border view for button
            UIView borderview = new UIView
            {
                Frame = baseView.Bounds
            };
            borderview.Layer.CornerRadius = 10;
            borderview.Layer.MasksToBounds = true;
            baseView.Transform = CGAffineTransform.MakeRotation((nfloat)(270.0 / 180 * Math.PI));

            // Stack subviews
            borderview.AddSubview(toolbar);
            baseView.AddSubview(borderview);
            nativeMap.AddSubview(baseView);
        }
    }
}
