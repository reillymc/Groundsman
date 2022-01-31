using System.ComponentModel;

namespace Groundsman.Models
{
    /// <summary>
    /// String representation of a position that plays nicely with Xamarin entries
    /// Inherits INotifyPropertyChanged so that Xamarin UI can detect changes (e.g. when matching close poly point to first point)
    /// </summary>
    public class DisplayPosition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _index;
        private string _longitude;
        private string _latitude;
        private string _altitude;

        public string Index
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

        public DisplayPosition(string index, string longitude, string latitude, string altitude)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
            Index = index;
        }

        public DisplayPosition(string index, Position position)
        {
            Longitude = position.Longitude.ToString();
            Latitude = position.Latitude.ToString();
            if (!double.IsNaN(position.Altitude))
            {
                Altitude = position.Altitude.ToString();
            }
            else
            {
                Altitude = "";
            }
            Index = index;
        }

        public DisplayPosition(string index, DisplayPosition position)
        {
            Longitude = position.Longitude;
            Latitude = position.Latitude;
            Altitude = position.Altitude;
            Index = index;
        }

        public bool IsBlank() => Latitude == "" && Longitude == "";

        public bool HasBlankCoordinate() => Latitude == "" && Longitude == "";

        public override string ToString() => $"{Index}, {Longitude}, {Latitude}, {Altitude}";

        public bool Equals(DisplayPosition comparePosition)
        {
            if (ReferenceEquals(this, comparePosition))
            {
                return true;
            }
            return Latitude == comparePosition.Latitude && Longitude == comparePosition.Longitude && Altitude == comparePosition.Altitude;
        }
    }
}
