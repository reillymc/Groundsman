using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

//Geodata.cs defines the models for geoJSON data to serialize into and from.
namespace Groundsman
{
    public class Feature
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public string name { get; set; }
        public string author { get; set; }
        public string date { get; set; }
        public string metadataStringValue { get; set; }
        public int metadataIntegerValue { get; set; }
        public float metadataFloatValue { get; set; }
        public string id { get; set; }
        [JsonIgnore]
        public ObservableCollection<Point> xamarincoordinates { get; set; }
        [JsonIgnore]
        public string typeIconPath { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<object> coordinates { get; set; }
    }

    public class RootObject : BindableObject
    {
        public string type { get; set; }

        public static readonly BindableProperty FeatureCollection
            =
         BindableProperty.Create("features", typeof(ObservableCollection<Feature>),
                                  typeof(RootObject),
                                  default(ObservableCollection<Feature>));

        public ObservableCollection<Feature> features
        {
            get { return (ObservableCollection<Feature>)GetValue(FeatureCollection); }
            set { SetValue(FeatureCollection, value); }
        }
    }
}
