using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Kazan_Session5_Mobile_21_9.GlobalClass;

namespace Kazan_Session5_Mobile_21_9
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WellInformation : ContentPage
    {
        long _wellId = 0;
        long _newId = 0;
        List<RockType> _rockTypeList;
        List<WellLayer> _toAddLayers = new List<WellLayer>();
        List<LayerView> _viewList = new List<LayerView>();
        List<WellLayer> _toRemoveLayers = new List<WellLayer>();
        List<WellLayer> _currentLayers = new List<WellLayer>();
        Well _well;
        public WellInformation(long id)
        {
            InitializeComponent();
            _wellId = id;
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickers();
            if (_wellId == 0)
            {
                var client = new WebApi();
                _newId = JsonConvert.DeserializeObject<long>(await client.PostAsync(null, "Wells/GetNewID"));
            }
            else
            {
                var client = new WebApi();
                _currentLayers = JsonConvert.DeserializeObject<List<WellLayer>>(await client.PostAsync(null, $"WellLayers/GetOriginalWellLayers?WellID={_wellId}"));
                _well = JsonConvert.DeserializeObject<Well>(await client.PostAsync(null, $"Wells/Details/{_wellId}"));
                foreach (var item in _currentLayers)
                {
                    _viewList.Add(new LayerView()
                    {
                        RockName = _rockTypeList.Where(x => x.ID == item.RockTypeID).Select(x => x.Name).FirstOrDefault(),
                        StartPoint = item.StartPoint,
                        EndPoint = item.EndPoint
                    });
                }
                _viewList = _viewList.OrderBy(x => x.StartPoint).ToList();
                lvLayers.ItemsSource = _viewList;
                entryCapacity.Text = _well.Capacity.ToString();
                entryDepth.Text = _well.GasOilDepth.ToString();
                entryWellName.Text = _well.WellName;
            }
        }

        private async Task LoadPickers()
        {
            var client = new WebApi();
            var rockTypeResponse = await client.PostAsync(null, "RockTypes");
            _rockTypeList = JsonConvert.DeserializeObject<List<RockType>>(rockTypeResponse);
            foreach (var item in _rockTypeList)
            {
                pRockLayers.Items.Add(item.Name);
            }
        }

        private async void btnAdd_Clicked(object sender, EventArgs e)
        {
            lvLayers.ItemsSource = null;
            if (pRockLayers.SelectedItem != null)
            {
                var getRockID = (from x in _rockTypeList
                                 where x.Name == pRockLayers.SelectedItem.ToString()
                                 select x.ID).FirstOrDefault();
                var startPoint = long.Parse(entryFrom.Text);
                var endPoint = long.Parse(entryTo.Text);
                if (_newId != 0)
                {
                    if (_viewList.Any(x => x.RockName == pRockLayers.SelectedItem.ToString()))
                    {
                        await DisplayAlert("Add Layers", "Well has already have added select layer!", "Ok");
                    }
                    else if (_viewList.Where(x => (startPoint > x.StartPoint && startPoint < x.EndPoint) || (endPoint > x.StartPoint && endPoint < x.EndPoint) || x.StartPoint == startPoint).Select(x => x).FirstOrDefault() != null)
                    {
                        await DisplayAlert("Add Layers", "Select layer will overlap existing layer(s)!", "Ok");
                    }
                    else if (startPoint > long.Parse(entryDepth.Text) || endPoint > long.Parse(entryDepth.Text))
                    {
                        await DisplayAlert("Add Layers", "Layer cannot exceed the value of “Depth of gas or oil extraction” of the well!", "Ok");
                    }
                    else if (endPoint - startPoint < 100)
                    {
                        await DisplayAlert("Add Layers", "Depth of layer must be at least 100 points!", "Ok");
                    }
                    else
                    {
                        var newRockLayer = new WellLayer()
                        {
                            WellID = _newId,
                            RockTypeID = getRockID,
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        };
                        _toAddLayers.Add(newRockLayer);
                        _viewList.Add(new LayerView()
                        {
                            RockName = pRockLayers.SelectedItem.ToString(),
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        });
                    }

                }
                else
                {
                    if (_viewList.Any(x => x.RockName == pRockLayers.SelectedItem.ToString()))
                    {
                        await DisplayAlert("Add Layers", "Well has already have added select layer!", "Ok");
                    }
                    else if (_viewList.Where(x => (startPoint > x.StartPoint && startPoint < x.EndPoint) || (endPoint > x.StartPoint && endPoint < x.EndPoint) || x.StartPoint == startPoint).Select(x => x).FirstOrDefault() != null)
                    {
                        await DisplayAlert("Add Layers", "Select layer will overlap existing layer(s)!", "Ok");
                    }
                    else if (startPoint > long.Parse(entryDepth.Text) || endPoint > long.Parse(entryDepth.Text))
                    {
                        await DisplayAlert("Add Layers", "Layer cannot exceed the value of “Depth of gas or oil extraction” of the well!", "Ok");
                    }
                    else if (endPoint - startPoint < 100)
                    {
                        await DisplayAlert("Add Layers", "Depth of layer must be at least 100 points!", "Ok");
                    }
                    else
                    {
                        var newRockLayer = new WellLayer()
                        {
                            WellID = _wellId,
                            RockTypeID = getRockID,
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        };
                        _toAddLayers.Add(newRockLayer);
                        _viewList.Add(new LayerView()
                        {
                            RockName = pRockLayers.SelectedItem.ToString(),
                            StartPoint = startPoint,
                            EndPoint = endPoint
                        });
                    }
                }
            }
            _viewList = _viewList.OrderBy(x => x.StartPoint).ToList();
            lvLayers.ItemsSource = _viewList;


        }

        private void btnRemove_Clicked(object sender, EventArgs e)
        {
            var removeBtn = (Button)sender;
            var stackLayout = (StackLayout)removeBtn.Parent;
            var childToTake = (StackLayout)stackLayout.Children[0];
            var lblRockName = (Label)childToTake.Children[0];
            var getRockID = (from x in _rockTypeList
                             where x.Name == lblRockName.Text
                             select x.ID).FirstOrDefault();
            if (_wellId != 0)
            {
                
                
                var findInCurrent = (from x in _currentLayers
                                     where x.RockTypeID == getRockID
                                     select x).FirstOrDefault();
                if (findInCurrent == null)
                {
                    var findInToAdd = (from x in _toAddLayers
                                       where x.RockTypeID == getRockID
                                       select x).FirstOrDefault();
                    _toAddLayers.Remove(findInToAdd);
                    _viewList.Remove(_viewList.Where(x => x.RockName == lblRockName.Text).Select(x => x).FirstOrDefault());
                }
                else
                {
                    _toRemoveLayers.Add(findInCurrent);
                    _viewList.Remove(_viewList.Where(x => x.RockName == lblRockName.Text).Select(x => x).FirstOrDefault());
                }
            }
            else
            {
                var findInToAdd = (from x in _toAddLayers
                                   where x.RockTypeID == getRockID
                                   select x).FirstOrDefault();
                _toAddLayers.Remove(findInToAdd);
                _viewList.Remove(_viewList.Where(x => x.RockName == lblRockName.Text).Select(x => x).FirstOrDefault());
            }
            lvLayers.ItemsSource = null;
            _viewList = _viewList.OrderBy(x => x.StartPoint).ToList();
            lvLayers.ItemsSource = _viewList;
        }

        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            if (_viewList.Any(x => x.StartPoint == 0))
            {
                var client = new WebApi();
                if (_wellId != 0)
                {
                    _well.GasOilDepth = long.Parse(entryDepth.Text);
                    _well.Capacity = long.Parse(entryCapacity.Text);
                    _well.WellName = entryWellName.Text;
                    var wellResponse = await client.PostAsync(JsonConvert.SerializeObject(_well), "Wells/Edit");
                    if (wellResponse == "\"Completed editing well!\"")
                    {
                        var boolCheck = true;
                        if (_toRemoveLayers.Count != 0)
                        {
                            foreach (var item in _toRemoveLayers)
                            {
                                var removeResponse = await client.PostAsync(JsonConvert.SerializeObject(item), "WellLayers/Delete");
                                if (removeResponse != "\"Well Layer removed!\"")
                                {
                                    boolCheck = false;
                                }
                            }
                            if (boolCheck == false)
                            {
                                await DisplayAlert("Submit", "An error occured during removing of layers! Please contact our administrator!", "Ok");
                            }
                            else
                            {
                                foreach (var item in _toAddLayers)
                                {
                                    var addResponse = await client.PostAsync(JsonConvert.SerializeObject(item), "WellLayers/Create");
                                    if (addResponse != "\"Create well layer!\"") 
                                    {
                                        boolCheck = false;
                                    }
                                }
                                if (boolCheck == false)
                                {
                                    await DisplayAlert("Submit", "An error occured during submission of new layers! Please contact our administrator!", "Ok");
                                }
                                else
                                {
                                    await DisplayAlert("Submit", "Changes submitted successfully!", "Ok");
                                    await Navigation.PopAsync();
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in _toAddLayers)
                            {
                                var addResponse = await client.PostAsync(JsonConvert.SerializeObject(item), "WellLayers/Create");
                                if (addResponse != "\"Create well layer!\"")
                                {
                                    boolCheck = false;
                                }
                            }
                            if (boolCheck == false)
                            {
                                await DisplayAlert("Submit", "An error occured during submission of new layers! Please contact our administrator!", "Ok");
                            }
                            else
                            {
                                await DisplayAlert("Submit", "Changes submitted successfully!", "Ok");
                                await Navigation.PopAsync();
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Submit", "An error occured when editing details of well! Please contact our adminstrator!", "Ok");
                    }
                }
                else
                {
                    var newWell = new Well()
                    {
                        ID = _newId,
                        WellTypeID = 1,
                        Capacity = long.Parse(entryCapacity.Text),
                        GasOilDepth = long.Parse(entryDepth.Text),
                        WellName = entryWellName.Text
                    };
                    var wellResponse = await client.PostAsync(JsonConvert.SerializeObject(newWell), "Wells/Create");
                    if (wellResponse == "\"Created Well conpleted!\"")
                    {
                        var boolCheck = true;
                        foreach (var item in _toAddLayers)
                        {
                            var addResponse = await client.PostAsync(JsonConvert.SerializeObject(item), "WellLayers/Create");
                            if (addResponse != "\"Create well layer!\"")
                            {
                                boolCheck = false;
                            }
                        }
                        if (boolCheck == false)
                        {
                            await DisplayAlert("Submit", "An error occured during submission of new layers! Please contact our administrator!", "Ok");
                        }
                        else
                        {
                            await DisplayAlert("Submit", "Changes submitted successfully!", "Ok");
                            await Navigation.PopAsync();
                        }
                    }
                    else
                    {
                        await DisplayAlert("Submit", "An error occured when editing details of well! Please contact our adminstrator!", "Ok");
                    }
                }
            }
            else
            {
                await DisplayAlert("Submit", "There must be a first layer of start point value of 0!", "Ok");
            }
        }

        private async void entryDepth_Completed(object sender, EventArgs e)
        {
            if (_wellId != 0)
            {
                var newDepth = long.Parse(entryDepth.Text);
                if (_viewList.Where(x => newDepth < x.StartPoint || newDepth < x.EndPoint).Select(x => x).FirstOrDefault() != null)
                {
                    await DisplayAlert("Edit Depth", "Unable to edit to new value as layer(s) will be more than assigned depth!", "Ok");
                    entryDepth.Text = _well.GasOilDepth.ToString();
                }
            }
        }

        private async void entryDepth_Unfocused(object sender, FocusEventArgs e)
        {
            if (_wellId != 0)
            {
                var newDepth = long.Parse(entryDepth.Text);
                if (_viewList.Where(x => newDepth < x.StartPoint || newDepth < x.EndPoint).Select(x => x).FirstOrDefault() != null)
                {
                    await DisplayAlert("Edit Depth", "Unable to edit to new value as layer(s) will be more than assigned depth!", "Ok");
                    entryDepth.Text = _well.GasOilDepth.ToString();
                }
            }
        }
    }
}