using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Kazan_Session5_Mobile_21_9.GlobalClass;

namespace Kazan_Session5_Mobile_21_9
{
    public partial class MainPage : ContentPage
    {
        List<Well> _wellList;
        List<GridView> _viewList;
        bool isConnected = true;
        public MainPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            checkConnectivity();

        }

        private void checkConnectivity()
        {
            try
            {
                checkNetwork();
                Timer timer = new Timer(
                (e)
               =>
                { MainThread.BeginInvokeOnMainThread(new Action(checkNetwork)); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {

                Title = "An error occured while attempting to connect to DataBase";
            }

        }

        private async void checkNetwork()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                isConnected = true;
                Title = "Connected to DataBase";
                btnAdd.IsEnabled = true;
                btnEdit.IsEnabled = true;
                if (pWell.SelectedItem != null)
                {
                    var previousSelected = pWell.SelectedItem.ToString();
                    await LoadPicker();
                    pWell.SelectedItem = previousSelected;
                }
                else
                {
                    await LoadPicker();
                }
            }
            else if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                isConnected = false;
                Title = "Disconnected to DataBase";
                btnAdd.IsEnabled = false;
                btnEdit.IsEnabled = false;
                if (pWell.SelectedItem != null)
                {
                    var previousSelected = pWell.SelectedItem.ToString();
                    await LoadPicker();
                    pWell.SelectedItem = previousSelected;
                }
                else
                {
                    await LoadPicker();
                }
            }
        }

        private async Task LoadPicker()
        {
            pWell.Items.Clear();
            stackView.Children.Clear();
            if (isConnected == true)
            {
                var client = new WebApi();
                var wellResponse = await client.PostAsync(null, "Wells");
                _wellList = JsonConvert.DeserializeObject<List<Well>>(wellResponse);
                File.WriteAllText(FileSystem.AppDataDirectory + "/wellPicker.txt", wellResponse);

            }
            else
            {
                if (File.Exists(FileSystem.AppDataDirectory + "/wellPicker.txt"))
                {
                    _wellList = JsonConvert.DeserializeObject<List<Well>>(File.ReadAllText(FileSystem.AppDataDirectory + "/wellPicker.txt"));
                }
                else
                {
                    await DisplayAlert("Loading Fields", "Please connect to the internet to retrieve latest copy of data!", "Ok");
                }
                
            }
            foreach (var item in _wellList)
            {
                pWell.Items.Add(item.WellName);
            }

        }

        private async void pWell_SelectedIndexChanged(object sender, EventArgs e)
        {
            stackView.Children.Clear();
            lblCapacity.Text = string.Empty;
            if (pWell.SelectedItem != null)
            {
                var getWell = (from x in _wellList
                               where x.WellName == pWell.SelectedItem.ToString()
                               select x).FirstOrDefault();
                lblCapacity.Text = $"{getWell.Capacity} m^3";
                if (isConnected)
                {
                    var client = new WebApi();
                    var wellLayersResponse = await client.PostAsync(null, $"WellLayers/GetWellLayers?WellID={getWell.ID}");
                    _viewList = JsonConvert.DeserializeObject<List<GridView>>(wellLayersResponse);
                    File.WriteAllText(FileSystem.AppDataDirectory + $"/{getWell.WellName}.txt", wellLayersResponse);
                    foreach (var item in _viewList)
                    {
                        double getProportion = (Convert.ToDouble(item.End - item.Start) / Convert.ToDouble(getWell.GasOilDepth)) * 400;


                        var stackLayout1 = new StackLayout()
                        {
                            Orientation = StackOrientation.Horizontal,
                            HeightRequest = getProportion,
                            BackgroundColor = Color.FromHex(item.BackgroundColour)
                        };
                        var stackLayout2 = new StackLayout() { Orientation = StackOrientation.Vertical };
                        var nameLabel = new Label()
                        {
                            Text = item.RockName,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                        };
                        var startLabel = new Label()
                        {
                            Text = item.Start.ToString(),
                            HorizontalOptions = LayoutOptions.End,
                            VerticalOptions = LayoutOptions.Start
                        };
                        var endLabel = new Label()
                        {
                            Text = item.End.ToString(),
                            HorizontalOptions = LayoutOptions.End,
                            VerticalOptions = LayoutOptions.EndAndExpand
                        };
                        stackLayout2.Children.Add(startLabel);
                        stackLayout2.Children.Add(endLabel);
                        stackLayout1.Children.Add(nameLabel);
                        stackLayout1.Children.Add(stackLayout2);
                        stackView.Children.Add(stackLayout1);


                    }
                    double getOilGasProportion = (Convert.ToDouble(getWell.GasOilDepth - _viewList.Last().End) / Convert.ToDouble(getWell.GasOilDepth)) * 400;
                    var gasOilStack = new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        HeightRequest = getOilGasProportion,
                        BackgroundColor = Color.Black
                    };
                    var gasOilLabel = new Label()
                    {
                        Text = "Oil/Gas",
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        TextColor = Color.White
                    };
                    gasOilStack.Children.Add(gasOilLabel);
                    stackView.Children.Add(gasOilStack);
                }
                else
                {
                    if (File.Exists(FileSystem.AppDataDirectory + $"/{getWell.WellName}.txt"))
                    {
                        _viewList = JsonConvert.DeserializeObject<List<GridView>>(File.ReadAllText(FileSystem.AppDataDirectory + $"/{getWell.WellName}.txt"));
                        foreach (var item in _viewList)
                        {
                            double getProportion = (Convert.ToDouble(item.End - item.Start) / Convert.ToDouble(getWell.GasOilDepth)) * 400;


                            var stackLayout1 = new StackLayout()
                            {
                                Orientation = StackOrientation.Horizontal,
                                HeightRequest = getProportion,
                                BackgroundColor = Color.FromHex(item.BackgroundColour)
                            };
                            var stackLayout2 = new StackLayout() { Orientation = StackOrientation.Vertical };
                            var nameLabel = new Label()
                            {
                                Text = item.RockName,
                                HorizontalOptions = LayoutOptions.CenterAndExpand,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                            };
                            var startLabel = new Label()
                            {
                                Text = item.Start.ToString(),
                                HorizontalOptions = LayoutOptions.End,
                                VerticalOptions = LayoutOptions.Start
                            };
                            var endLabel = new Label()
                            {
                                Text = item.End.ToString(),
                                HorizontalOptions = LayoutOptions.End,
                                VerticalOptions = LayoutOptions.EndAndExpand
                            };
                            stackLayout2.Children.Add(startLabel);
                            stackLayout2.Children.Add(endLabel);
                            stackLayout1.Children.Add(nameLabel);
                            stackLayout1.Children.Add(stackLayout2);
                            stackView.Children.Add(stackLayout1);


                        }
                        double getOilGasProportion = (Convert.ToDouble(getWell.GasOilDepth - _viewList.Last().End) / Convert.ToDouble(getWell.GasOilDepth)) * 400;
                        var gasOilStack = new StackLayout()
                        {
                            Orientation = StackOrientation.Horizontal,
                            HeightRequest = getOilGasProportion,
                            BackgroundColor = Color.Black
                        };
                        var gasOilLabel = new Label()
                        {
                            Text = "Oil/Gas",
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            TextColor = Color.White
                        };
                        gasOilStack.Children.Add(gasOilLabel);
                        stackView.Children.Add(gasOilStack);
                    }
                    else
                    {
                        await DisplayAlert("Loading Fields", "Please connect to the internet to retrieve latest copy of data!", "Ok");
                    }
                }
                
                
            }

        }

        private async void btnEdit_Clicked(object sender, EventArgs e)
        {

            if (pWell.SelectedItem != null)
            {
                var getWell = (from x in _wellList
                               where x.WellName == pWell.SelectedItem.ToString()
                               select x).FirstOrDefault();
                await Navigation.PushAsync(new WellInformation(getWell.ID));
            }
            else
            {
                await DisplayAlert("Edit Well", "Please select a Well!", "Ok");
            }
        }

        private async void btnAdd_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WellInformation(0));
        }
    }
}
