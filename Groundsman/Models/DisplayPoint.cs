using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Groundsman.Models
{
    public class DisplayPoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _index;
        private string _latitude;
        private string _longitude;
        private string _altitude;

        public int Index
        {
            get { return _index; }
            set { _index = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Index")); }
        }

        public string Latitude
        {
            get { return _latitude; }
            set { _latitude = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Latitude")); }
        }

        public string Longitude
        {
            get { return _longitude; }
            set { _longitude = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Longitude")); }
        }

        public string Altitude
        {
            get { return _altitude; }
            set { _altitude = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Altitude")); }
        }

        public DisplayPoint(int index, string lat, string lng, string alt)
        {
            Latitude = lat;
            Longitude = lng;
            Altitude = alt;
            Index = index;
        }
    }
}
