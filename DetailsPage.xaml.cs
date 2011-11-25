using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Device.Location;
using RestArea.Common;

namespace RestArea
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private GeoCoordinateWatcher watcher = null;
        private GeoCoordinate location = null;

        public DetailsPage()
        {
            InitializeComponent();

            // initialize the location service
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            watcher.MovementThreshold = 200;
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);

            watcher.Start();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.PageTitle.Text = NavigationContext.QueryString["name"];
            this.txtDescription.Text = NavigationContext.QueryString["description"];

            if (NavigationContext.QueryString["options"].Contains("RR"))
            {
                this.imgRestrooms.Opacity = 1.0;
                this.txtRestrooms.Opacity = 1.0;
            }
            if (NavigationContext.QueryString["options"].Contains("VM"))
            {
                this.imgVending.Opacity = 1.0;
                this.txtVending.Opacity = 1.0;
            }
            if (NavigationContext.QueryString["options"].Contains("Food"))
            {
                this.imgFood.Opacity = 1.0;
                this.txtFood.Opacity = 1.0;
            }
            if (NavigationContext.QueryString["options"].Contains("Gas"))
            {
                this.imgGas.Opacity = 1.0;
                this.txtGas.Opacity = 1.0;
            }
            if (NavigationContext.QueryString["options"].Contains("HF"))
            {
                this.imgHandicap.Opacity = 1.0;
                this.txtHandicap.Opacity = 1.0;
            }
            if (NavigationContext.QueryString["options"].Contains("RV"))
            {
                this.imgRV.Opacity = 1.0;
                this.txtRV.Opacity = 1.0;
            }
            if (NavigationContext.QueryString["options"].Contains("PT"))
            {
                this.imgPicnic.Opacity = 1.0;
                this.txtPicnic.Opacity = 1.0;
            }
            if (NavigationContext.QueryString["options"].Contains("Pets"))
            {
                this.imgPets.Opacity = 1.0;
                this.txtPets.Opacity = 1.0;
            }

            location = new GeoCoordinate(Convert.ToDouble(NavigationContext.QueryString["lat2"]), Convert.ToDouble(NavigationContext.QueryString["lon2"]));

            base.OnNavigatedTo(e);
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            // calculate distances
            var dist = Utilities.ConvertToMiles(Utilities.Distance(e.Position.Location.Latitude, e.Position.Location.Longitude, location.Latitude, location.Longitude));
            this.txtDescription.Text = dist.ToString("0.00") + " miles away";
        }

        private void txtDescription_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BingMapsDirectionsTask bingMapsDirectionsTask = new BingMapsDirectionsTask();

            LabeledMapLocation start = new LabeledMapLocation("My Location", new GeoCoordinate(Convert.ToDouble(NavigationContext.QueryString["lat1"]), Convert.ToDouble(NavigationContext.QueryString["lon1"])));
            bingMapsDirectionsTask.Start = start;

            LabeledMapLocation end = new LabeledMapLocation(NavigationContext.QueryString["name"], new GeoCoordinate(Convert.ToDouble(NavigationContext.QueryString["lat2"]), Convert.ToDouble(NavigationContext.QueryString["lon2"])));
            bingMapsDirectionsTask.End = end;

            bingMapsDirectionsTask.Show();
        }
    }
}