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

using System.Device.Location;
using RestArea.Common;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace RestArea
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ProgressIndicator progressIndicator = null;
        private GeoCoordinateWatcher watcher = null;
        private List<RestAreaModel> database = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // load the data
            database = new List<RestAreaModel>();
            this.LoadDatabase();

            // initialize the location service
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            watcher.MovementThreshold = 200;
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);

            if (IsolatedStorageSettings.ApplicationSettings.Contains("IsLocationEnabled") == true)
            {
                if ((bool)IsolatedStorageSettings.ApplicationSettings["IsLocationEnabled"] == true)
                {
                    watcher.Start();
                    IsolatedStorageSettings.ApplicationSettings["IsLocationEnabled"] = true;
                    ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = "turn off location";
                    
                    this.mapRestArea.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["IsLocationEnabled"] = false;
                    ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = "turn on location";

                    this.mapRestArea.Visibility = System.Windows.Visibility.Collapsed;
                    MessageBox.Show("You have disabled location services for this application. Rest Area works best when this feature is enabled.", "Location Disabled", MessageBoxButton.OK);
                }
            }
            else
            {
                watcher.Start();
                IsolatedStorageSettings.ApplicationSettings["IsLocationEnabled"] = true;
                ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = "turn off location";

                this.mapRestArea.Visibility = System.Windows.Visibility.Visible;
            }

            Pivot_SelectionChanged(this.pivRestArea, null);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationService.BackStack.Count<System.Windows.Navigation.JournalEntry>() > 0 &&
                NavigationService.BackStack.Last<System.Windows.Navigation.JournalEntry>().Source.ToString().Contains("WelcomePage.xaml"))
            {
                NavigationService.RemoveBackEntry();
            }

            // attach progress indicator
            if (progressIndicator == null)
            {
                progressIndicator = new ProgressIndicator();
                progressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator = progressIndicator;
            }
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Disabled)
            {
                MessageBox.Show("Location services are not enabled for your phone. Rest Area works best when this feature is enabled.", "Location Disabled", MessageBoxButton.OK);
            }
            else if (e.Status == GeoPositionStatus.NoData)
            {
                MessageBox.Show("Your location could not be determined, please try again later. Rest Area will use your most recent location until a new location is determined.", "Location Unavailable", MessageBoxButton.OK);
            }
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (watcher.Status == GeoPositionStatus.Ready &&
                e.Position.Location.IsUnknown == false)
            {
                this.EnableProgressBar();

                // clear the map
                this.mapRestArea.Children.Clear();

                // calculate distances
                foreach (RestAreaModel r in database)
                {
                    r.Distance = Utilities.ConvertToMiles(Utilities.Distance(e.Position.Location.Latitude, e.Position.Location.Longitude, r.Latitude, r.Longitude));
                    r.Description = r.Distance.ToString("0.00") + " miles away";
                }

                List<RestAreaModel> refined = database.OrderBy(r => r.Distance).Take(25).ToList();
                foreach (RestAreaModel r in refined.Reverse<RestAreaModel>())
                {
                    Pushpin p = new Pushpin();
                    p.Location = new GeoCoordinate(r.Latitude, r.Longitude);
                    p.Background = (SolidColorBrush)Resources["PhoneAccentBrush"];
                    p.Content = r.Name;
                    p.DataContext = r;
                    p.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(Pushpin_Tap);

                    this.mapRestArea.Children.Add(p);
                }

                this.lstRestArea.ItemsSource = refined;

                // show current location
                Pushpin me = new Pushpin();
                me.Location = e.Position.Location;
                me.Content = "My Location";

                this.mapRestArea.Children.Add(me);

                Map_MapPan(null, null);
            }
        }

        private void Map_MapResolved(object sender, EventArgs e)
        {
            this.DisableProgressBar();
        }

        private void Map_MapPan(object sender, MapDragEventArgs e)
        {
            if (this.mapRestArea.IsDownloading == false)
                this.DisableProgressBar();
            else
                this.EnableProgressBar();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Pivot)sender).SelectedItem == this.pviMapView)
            {
                this.EnableProgressBar();
                this.mapRestArea.SetView(watcher.Position.Location, 10.0);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Pivot_SelectionChanged(this.pivRestArea, null);
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RestAreaModel r = (RestAreaModel)((TextBlock)e.OriginalSource).DataContext;

            NavigationService.Navigate(new Uri("/DetailsPage.xaml?name=" + r.Name + "&description=" + r.Description + "&options=" + r.Options + "&lat1=" + watcher.Position.Location.Latitude + "&lon1=" + watcher.Position.Location.Longitude + "&lat2=" + r.Latitude + "&lon2=" + r.Longitude, UriKind.Relative));
        }

        private void Pushpin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RestAreaModel r = (RestAreaModel)((Pushpin)sender).DataContext;

            NavigationService.Navigate(new Uri("/DetailsPage.xaml?name=" + r.Name + "&description=" + r.Description + "&options=" + r.Options + "&lat1=" + watcher.Position.Location.Latitude + "&lon1=" + watcher.Position.Location.Longitude + "&lat2=" + r.Latitude + "&lon2=" + r.Longitude, UriKind.Relative));
        }

        private void mnuToggleLocation_Click(object sender, EventArgs e)
        {
            if (((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text == "turn off location")
            {
                watcher.Stop();
                IsolatedStorageSettings.ApplicationSettings["IsLocationEnabled"] = false;
                ((ApplicationBarMenuItem)this.ApplicationBar.MenuItems[0]).Text = "turn on location";

                this.mapRestArea.Visibility = System.Windows.Visibility.Collapsed;
                MessageBox.Show("You have disabled location services for this application. Rest Area works best when this feature is enabled.", "Location Disabled", MessageBoxButton.OK);
            }
            else
            {
                int index = new Random().Next(10000, 99999);

                IsolatedStorageSettings.ApplicationSettings["IsLocationEnabled"] = true;
                NavigationService.Navigate(new Uri("/MainPage.xaml?r=" + index, UriKind.Relative));
            }
        }

        private void EnableProgressBar()
        {
            if (progressIndicator != null)
                progressIndicator.IsIndeterminate = true;
        }

        private void DisableProgressBar()
        {
            if (progressIndicator != null)
                progressIndicator.IsIndeterminate = false;
        }

        private void LoadDatabase()
        {
            var tmp = CSVReader.FromStream(Application.GetResourceStream(new Uri("Model/database.csv", UriKind.Relative)).Stream);

            foreach (List<string> row in tmp)
            {
                RestAreaModel r = new RestAreaModel();

                r.Latitude = Convert.ToDouble(row[1]);
                r.Longitude = Convert.ToDouble(row[0]);
                r.Name = row[2].Replace("REST AREA-", "").Replace("REST AREA", "").Replace("WELCOME CENTER-", "").Replace("WELCOME CENTER", "").Replace("SERVICE PLAZA-", "").Replace("SERVICE PLAZA", "").Replace("TRUCK PARKING", "").Replace("TRUCK", "").Replace("-PARKING", "").Replace("FOLLOW SIGNS", "").Replace("LEFT EXIT", "").Replace("TURNOUT", "").Trim();
                r.Options = row[3];

                database.Add(r);
            }
        }
    }
}