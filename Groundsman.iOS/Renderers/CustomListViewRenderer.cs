using Groundsman.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ListView), typeof(CustomListViewRenderer))]
namespace Groundsman.iOS.Renderers;

public class CustomListViewRenderer : ListViewRenderer
{
    protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
    {
        base.OnElementChanged(e);
        if (e.NewElement != null)
        {
            UITableView listView = Control;
            listView.SeparatorInset = new UIEdgeInsets(0, 63, 0, 0);
        }
    }
}
