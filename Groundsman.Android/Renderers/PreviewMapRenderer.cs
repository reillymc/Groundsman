using Android.Content;
using Android.Gms.Maps;
using Groundsman;
using Groundsman.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(PreviewMap), typeof(PreviewMapRenderer))]
namespace Groundsman.Droid.Renderers
{
    public class PreviewMapRenderer : MapRenderer

    {
        public PreviewMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);
            map.UiSettings.ZoomControlsEnabled = false;
        }
    }
}
