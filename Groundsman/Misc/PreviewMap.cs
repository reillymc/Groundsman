using Xamarin.Forms.Maps;

namespace Groundsman;

public class PreviewMap : Map
{
    public PreviewMap() : base() { }
    public PreviewMap(MapSpan span) : base(span) { }
}