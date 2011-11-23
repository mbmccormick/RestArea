using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Device.Location;

namespace RestArea
{
    public class RestAreaModel
    {
        public double Latitude
        {
            get;
            set;
        }

        public double Longitude
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Options
        {
            get;
            set;
        }

        public string Distance
        {
            get;
            set;
        }

        public GeoCoordinate Location
        {
            get
            {
                return new GeoCoordinate(this.Latitude, this.Longitude);
            }
        }
    }
}
