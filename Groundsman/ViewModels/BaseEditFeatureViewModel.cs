using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Groundsman.Misc;
using Groundsman.Models;
using Groundsman.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Point = Groundsman.Models.Point;
using Polygon = Groundsman.Models.Polygon;
using Position = Groundsman.Models.Position;
using XFMPosition = Xamarin.Forms.Maps.Position;

namespace Groundsman.ViewModels
{
    public abstract class BaseEditFeatureViewModel : BaseViewModel
    {
        public ICommand OnDoneTappedCommand { get; set; }
        public ICommand OnCancelTappedCommand { get; set; }
        public ICommand ShareButtonClickCommand { get; set; }
        public ICommand GetFeatureCommand { get; set; }
        public ICommand DeleteFeatureCommand { get; set; }

        public GeoJSONType GeometryType;

        public readonly Feature Feature = new Feature { Type = GeoJSONType.Feature };
        public ObservableCollection<DisplayPosition> Positions { get; set; } = new ObservableCollection<DisplayPosition>();

        public bool IsExistingFeature { get; set; } = false;
        public bool ShowMapPreview { get; set; } = true;

        public string NameEntry { get; set; }
        public string DateEntry { get; set; }

        public PreviewMap Map { get; private set; }

        public BaseEditFeatureViewModel()
        {
            OnDoneTappedCommand = new Command(async () => await SaveDismiss());
            ShareButtonClickCommand = new Command<View>(async (view) => await ShareFeature(view));
            OnCancelTappedCommand = new Command(async () => await CancelDismiss());
            DeleteFeatureCommand = new Command(async () => await DeleteDismiss());

            Map = new PreviewMap
            {
                InputTransparent = true,
            };

            if (!Preferences.Get(Constants.MapPreviewKey, true))
            {
                ShowMapPreview = false;
            }

            Positions.CollectionChanged += Positions_CollectionChanged;
        }

        private void Positions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Altitude")
            {
                UpdateMap();
            }
        }

        public void CenterMap(Position position, Position span)
        {
            if (position == null || span == null) return;
            Map.MoveToRegion(new MapSpan(new XFMPosition(position.Latitude, position.Longitude), span.Latitude, span.Longitude));
        }

        public async void UpdateMap()
        {
            if (!Preferences.Get(Constants.MapPreviewKey, true)) return;

            Map.MapElements.Clear();
            Map.Pins.Clear();

            Position centerPosition;

            try
            {
                centerPosition = await HelperServices.GetGeoLocation();
            }
            catch
            {
                centerPosition = new Position(0, 0);
            }


            Position spanPosition = new Position(0.01, 0.01);

            if (Positions.Count > 0)
            {
                try
                {
                    switch (GeometryType)
                    {
                        case GeoJSONType.Point:
                            var point = (Point)FeatureHelper.GetValidatedGeometry(Positions, GeoJSONType.Point);
                            centerPosition = new Position(point.Coordinates.Longitude, point.Coordinates.Latitude);
                            Map.Pins.Add(MapHelper.GeneratePin(new Feature(FeatureHelper.GetValidatedGeometry(Positions, GeoJSONType.Point))));
                            break;
                        case GeoJSONType.LineString:
                            var line = (LineString)FeatureHelper.GetValidatedGeometry(Positions, GeoJSONType.LineString);
                            centerPosition = line.GetCenterPosition();
                            spanPosition = line.GetSpan();
                            Map.MapElements.Add(MapHelper.GenerateLine(new Feature(FeatureHelper.GetValidatedGeometry(Positions, GeoJSONType.LineString))));
                            break;
                        case GeoJSONType.Polygon:
                            var closedPositions = Positions.ToList();
                            var polygon = (Polygon)FeatureHelper.GetValidatedGeometry(Positions, GeoJSONType.Polygon);
                            closedPositions.Add(Positions[0]);
                            centerPosition = polygon.GetCenterPosition();
                            spanPosition = polygon.GetSpan();
                            Map.MapElements.Add(MapHelper.GeneratePolygon(new Feature(FeatureHelper.GetValidatedGeometry(Positions, GeoJSONType.Polygon))));
                            break;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            CenterMap(centerPosition, spanPosition);
        }

        public abstract Task ShareFeature(View view);

        public abstract Task SaveDismiss();

        public abstract Task CancelDismiss();

        public abstract void OnAppear();
        public abstract void OnDisappear();
        public abstract Task DeleteDismiss();
    }
}
