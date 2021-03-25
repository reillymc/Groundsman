using System.ComponentModel;

namespace Groundsman.Models
{
    /// <summary>
    /// String representation of a position that plays nicely with Xamarin entries
    /// </summary>
    public class DisplayPosition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _index;
        private string _longitude;
        private string _latitude;
        private string _altitude;

        public int Index
        {
            get => _index;
            set { _index = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Index")); }
        }

        public string Longitude
        {
            get => _longitude;
            set { _longitude = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Longitude")); }
        }

        public string Latitude
        {
            get => _latitude;
            set { _latitude = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Latitude")); }
        }

        public string Altitude
        {
            get => _altitude;
            set { _altitude = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Altitude")); }
        }

        public DisplayPosition(int index, string longitude, string latitude, string alt)
        {
            Longitude = latitude;
            Latitude = longitude;
            Altitude = alt;
            Index = index;
        }

        public DisplayPosition(int index, Position position)
        {
            Longitude = position.Longitude.ToString();
            Latitude = position.Latitude.ToString();
            Altitude = position.Altitude.ToString();
            Index = index;
        }
        public DisplayPosition(int index, DisplayPosition position)
        {
            Longitude = position.Longitude.ToString();
            Latitude = position.Latitude.ToString();
            Altitude = position.Altitude.ToString();
            Index = index;
        }
    }
}
