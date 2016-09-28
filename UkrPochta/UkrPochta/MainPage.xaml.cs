using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UkrPochta.Data;
using UkrPochta.Data.Service;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.UI.Xaml.Controls.Maps;
using System.Collections;
using UkrPochta.Data.Entity;
using Microsoft.HockeyApp;

namespace UkrPochta
{
    
    public sealed partial class MainPage : Page
    {
        private StorageFolder LocalFolder { get; } = ApplicationData.Current.LocalFolder;
        private StorageFolder TempFolder { get; } = ApplicationData.Current.TemporaryFolder;
        private const string DbName = "ukrPochta.sqlite";
        private const string PathDbFileAssets = @"ms-appx:///Assets/ukrPochta.sqlite";
        private DB db;
        private IEnumerable<Base> regions= new List<Region>();
        private ChooseAddr _adrr=new ChooseAddr();   
        
        public MainPage()
        {
            this.InitializeComponent();
        }


        private async void RefreshClick(object sender, TappedRoutedEventArgs e)
        {
            ProgressStep.IsActive = true;
            StatusText.Text = "download Data";
            var LoggedText = new Progress<string>();
            LoggedText.ProgressChanged += OnProgressChanged;
            var x=  await db.UpdateDate(TempFolder,LoggedText);
            if (!x)
                StatusText.Text = "not download Data";
            else StatusText.Text = "ready";
            ProgressStep.IsActive = false;
        }

        private void OnProgressChanged(object sender, string e)
        {
            StatusText.Text = e;
        }

        private async void LoadedPage(object sender, RoutedEventArgs e)
        {
            ProgressStep.IsActive = true;
           await CheckDB();
            if (db == null || db.IsEmpty)
                StatusText.Text = "you must download the database";
            else
               StatusText.Text = "ready";
            ProgressStep.IsActive = false;

            regions = db.GetRegion();
            ListRgn.Obj = regions;
            ListRgn.TypeObj = new Region().GetType();

        }



        private  async Task CheckDB()
        {
            var x = await LocalFolder.TryGetItemAsync(DbName);          
            if (x == null)
            {
                StatusText.Text = "UnPacked db file";
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(PathDbFileAssets));
                var xs= await file.CopyAsync(LocalFolder, DbName);
                if (xs != null)
                {
                    var f = await LocalFolder.GetFileAsync(DbName);
                    if (f!=null)
                    db = new DB(f.Path);
                }
            }
            else
            db = new DB(x.Path);          
        }

        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            

        }
                  
        

        private async void MapsGoTo(string name)
        {
            string addressToGeocode = name;
            Mapa.MapElements.Clear();
            ListFind.Visibility = Visibility.Visible;
            ListFind.Items.Clear();
            if (string.IsNullOrEmpty(addressToGeocode))
                return;
            BasicGeoposition qHint = new BasicGeoposition();
            qHint.Latitude = 0;
            qHint.Longitude = 0;
            Geopoint hintPoint = new Geopoint(qHint);
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(addressToGeocode, hintPoint, 10);
            StatusText.Text = "Find: " + addressToGeocode + " (" + result.Locations.Count + ")";
            if (result.Locations.Count == 0)
                return;
            
            if (result.Status == MapLocationFinderStatus.Success)
            {
                double Lat = 0;
                double Long = 0;
                foreach(MapLocation mp in result.Locations)
                {
                    Lat = mp.Point.Position.Latitude;
                    Long = mp.Point.Position.Longitude;
                    
                    BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = Lat, Longitude = Long };
                    Geopoint cityCenter = new Geopoint(cityPosition);
                    MapIcon mIcon = new MapIcon();
                    mIcon.Location = cityCenter;
                    mIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                    
                    var t = mp.Address.FormattedAddress+"("+ Lat.ToString() + " - " + Long.ToString() + ")";// +","+ mp.Address.Country + "," + mp.Address.Region + "," + mp.Address.District + "," + mp.Address.Town + "," + mp.Address.Street + "," + mp.Address.StreetNumber;
                    mIcon.Title = t;//addressToGeocode;
                    mIcon.ZIndex = 0;
                    ListFind.Items.Add(mp);
                    Mapa.MapElements.Add(mIcon);
                    Mapa.Center = cityCenter;
                    Mapa.ZoomLevel = 5;
                    Mapa.LandmarksVisible = true;
                }
                if(ListFind.Items.Count() == 0)
                {
                    ListFind.Visibility = Visibility.Collapsed;
                }
            }

                       
        }
               
             

       

        private void Changed(object sender, object e)
        {
            if(sender is ListDataControl)
            {
                if ((e == null))
                {
                    (sender as ListDataControl).Visibility = Visibility.Collapsed;
                    FillDependedObject((sender as ListDataControl).Tag, null);
                    // return;
                }
                else
                {
                    FillDependedObject((sender as ListDataControl).Tag, (e as Base));                    
                }
                MapsGoTo(_adrr.ChooseAdrr);
            }
        }
        private void FillDependedObject(object tag,Base obj)
        {
            if (obj != null)
                _adrr.ChooseStreet = obj.Name;
            if (tag == null||obj==null)
                return;
            
            var id = (int)obj.Id;
            var control=FindName((tag as ListDataControl).Name);
            if (control == null)
                return;
            if ((control as ListDataControl).Visibility == Visibility.Collapsed)
                (control as ListDataControl).Visibility = Visibility.Visible;
            if ((control as ListDataControl).Name.Contains("Distr"))
            {
                (control as ListDataControl).Obj = (IEnumerable<Base>)db.GetDistrict(id);
                _adrr.ChooseRegion = obj.Name;
            }
                
            if ((control as ListDataControl).Name.Contains("City"))
            {
                (control as ListDataControl).Obj = (IEnumerable<Base>)db.GetCity(id);
                _adrr.ChooseDistrict = obj.Name;
            }
               
            if ((control as ListDataControl).Name.Contains("Street"))
            {
                (control as ListDataControl).Obj = (IEnumerable<Base>)db.GetStreet(id);
                _adrr.ChooseCity = obj.Name;
            }
                
            
        }
        private void VChenged(object sender, bool visible)
        {
            var control = (ListDataControl)sender;
            if (visible)
            {
                foreach(UIElement el in grdList.Children)
                {
                    if(el is ListDataControl)
                    {
                        if ((el as ListDataControl).Equals(control) && control.Tag != null)
                        {
                            (el as ListDataControl).Height = 72;
                            var row=Grid.GetRow((FrameworkElement)el);
                            var cr=grdList.RowDefinitions;
                            cr[row].Height = GridLength.Auto;
                            row = Grid.GetRow((FrameworkElement)control.Tag);
                            cr[row].Height= new GridLength(1, GridUnitType.Star);

                        }

                        //    (el as ListDataControl).Width = 50;
                    }
                }
               // control.Width = 350;
            }
            else
            {
                if (control.Tag != null && (control.Tag as ListDataControl).Obj != null)
                {
                    (control.Tag as ListDataControl).Obj = null;
                }
                foreach (UIElement el in grdList.Children)
                {
                    if (el is ListDataControl)
                    {
                        if ((el as ListDataControl).Tag != null && (el as ListDataControl).Tag.Equals(control))
                        {
                            (el as ListDataControl).Height = double.NaN;
                            var row = Grid.GetRow((FrameworkElement)el);
                            var cr = grdList.RowDefinitions;
                            for(int i = 0; i < cr.Count; i++)
                            {
                                if (i == row)
                                    cr[row].Height = new GridLength(1, GridUnitType.Star);
                                else
                                    cr[i].Height = GridLength.Auto;
                            }

                            
                           // break;
                        }
                    
                          //  (el as ListDataControl).Width = 350;
                    }
                }

            }

                

        }             
        private void ClickFind(object sender, ItemClickEventArgs e)
        {
            var mp = (MapLocation)e.ClickedItem;
            Mapa.Center = mp.Point;
            Mapa.ZoomLevel = 12;

        }

        private void CloseClick(object sender, TappedRoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
