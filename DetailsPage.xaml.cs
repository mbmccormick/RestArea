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

namespace RestArea
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        public DetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.PageTitle.Text = NavigationContext.QueryString["name"];
            this.txtDistance.Text = NavigationContext.QueryString["distance"];

            if (NavigationContext.QueryString["options"].Contains("RR"))
                this.imgRestrooms.Opacity = 1.0;
            if (NavigationContext.QueryString["options"].Contains("RV"))
                this.imgRV.Opacity = 1.0;
            if (NavigationContext.QueryString["options"].Contains("PT"))
                this.imgPicnic.Opacity = 1.0;
            if (NavigationContext.QueryString["options"].Contains("Pets"))
                this.imgPets.Opacity = 1.0;
            if (NavigationContext.QueryString["options"].Contains("HF"))
                this.imgHandicap.Opacity = 1.0;
            if (NavigationContext.QueryString["options"].Contains("VM"))
                this.imgVending.Opacity = 1.0;
            if (NavigationContext.QueryString["options"].Contains("Food"))
                this.imgFood.Opacity = 1.0;
            if (NavigationContext.QueryString["options"].Contains("Gas"))
                this.imgGas.Opacity = 1.0;

            base.OnNavigatedTo(e);
        }

        private void txtDistance_Tap(object sender, GestureEventArgs e)
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