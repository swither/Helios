//  Copyright 2023 Helios Contributors
//
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using GadrocsWorkshop.Helios.Windows;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Interfaces
{
    using GadrocsWorkshop.Helios.Windows.Controls;
	using NOR.FrontDoor.Client;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Configuration;
	using System.Net;
	using System.Net.Sockets;
	using System.Runtime.Remoting.Messaging;
	using System.Windows;
	using System.Windows.Controls;




	/// <summary>
	/// Interaction logic for NORInterfaceEditor.xaml
	/// </summary>
	public partial class NORInterfaceEditor : HeliosInterfaceEditor
    {
		NORInterface _parent = null;


		public string Address
		{
			get => _parent.Address;
			set
			{
				if (_parent.Address == null && value != null
					|| _parent.Address != null && !_parent.Address.Equals(value))
				{
					string oldValue = _parent.Address;
					_parent.Address = value;
				}
			}
		}



		public NORInterfaceEditor()
		{
		}

		/// <summary>
		/// Called immediately after construction when our factory installs the Interface property, not
		/// executed at run time (Profile Editor only)
		/// </summary>
		protected override void OnInterfaceChanged(HeliosInterface oldInterface, HeliosInterface newInterface)
		{
			base.OnInterfaceChanged(oldInterface, newInterface);
		
			if (newInterface is NORInterface norInterface)
			{
				_parent = norInterface;
			}
			else
			{
				throw new NotImplementedException();
			}


			InitializeComponent();
			PopulateList();
		}



		private RelayCommand configureCommand;

		public ICommand ConfigureCommand
		{
			get
			{
				if (configureCommand == null)
				{
					configureCommand = new RelayCommand(Configure);
				}

				return configureCommand;
			}
		}

		private void PopulateList()
		{
			Properties.Items.Clear();

			//Clickables
			if (_parent.ClickablesCache != null)
			{
				var ClickablesItem = new TreeViewItem();
				ClickablesItem.Header = "Clickables";
				ClickablesItem.IsExpanded = false;
				Properties.Items.Add(ClickablesItem);

				foreach (var Clickable in _parent.ClickablesCache)
				{
					var item = new TreeViewItem();
					item.Header = $"{Clickable.Item1}";
					item.IsExpanded = false;
					ClickablesItem.Items.Add(item);
				}
			}

			//Properties
			if (_parent.ComponentsCache != null)
			{
				foreach (var Component in _parent.ComponentsCache)
				{
					var ComponentItem = new TreeViewItem();
					ComponentItem.Header = Component.Key;
					ComponentItem.IsExpanded = false;
					Properties.Items.Add(ComponentItem);

					foreach (var CompPropertyGroup in Component.Value)
					{
						var ComponentStateType = new TreeViewItem();
						ComponentStateType.Header = CompPropertyGroup.Key;
						ComponentStateType.IsExpanded = false;
						ComponentItem.Items.Add(ComponentStateType);

						foreach (var CompProperty in CompPropertyGroup.Value)
						{
							var PropertyItem = new TreeViewItem();
							PropertyItem.Header = CompProperty.Item1;
							PropertyItem.IsExpanded = false;
							ComponentStateType.Items.Add(PropertyItem);
						}
					}
				}
			}
		}

		private async void Configure(object commandParameter)
		{
			ConfigureButton.IsEnabled = false;

			GrabStatus.Text = "Connecting";

			//Create frontdoor client
			var FDClient = new FrontDoorClient(Address);

			var Player = await FDClient.GetPlayerState();


			if (!Player.ContainsKey("Possessed"))
			{
				return;
			}
			var Possessed = Player["Possessed"];

			if (!Possessed.ContainsKey("ActorPath"))
			{
				return;
			}

			if (!Possessed.ContainsKey("TypeName"))
			{
				return;
			}
			_parent.TypeName = Possessed["TypeName"].StringValue;

			var PlayerPath = Possessed["ActorPath"];

			GrabStatus.Text = "Grabbing Clickables";

			//Grab all clickable states and then start streaming them for updates
			var ClickablesDict = await FDClient.GetClickableStates();
			_parent.ClickablesCache = new List<Tuple<String, String>>();
			foreach (var ClickableGroup in ClickablesDict)
			{
				foreach (var Clickable in ClickableGroup.Value)
				{
					if (Clickable.Value is NOR.FrontDoor.Client.PropertyValue.DoublePropertyValue)
					{
						_parent.ClickablesCache.Add(new Tuple<String, String> ( Clickable.Key, BindingValueUnits.FetchUnitName(BindingValueUnits.Numeric)));
					}
					else
					{
						_parent.ClickablesCache.Add(new Tuple<String, String>(Clickable.Key, BindingValueUnits.FetchUnitName(BindingValueUnits.Text)));
					}

				}
			}

			GrabStatus.Text = "Grabbing Component Properties";

			//Grab all components
			var TempCache = await FDClient.GetComponents();

			_parent.ComponentsCache = new Dictionary<string, Dictionary<string, List<Tuple<string, string>>>>();
			foreach (var Component in TempCache)
			{
				if (Component.Contains(PlayerPath.StringValue))
				{
					String StrippedComp = Component.Replace(PlayerPath.StringValue + ".", "");
					GrabStatus.Text = $"Grabbing Component Properties: {StrippedComp}";

					var CompProperties = await FDClient.GetComponentProperties(Component);

					var ComponentDict = new Dictionary<string, List<Tuple<string, string>>>();

					foreach (var CompPropertyGroup in CompProperties)
					{
						var PropertyGroupList = new List<Tuple<string, string>>();

						foreach (var CompProperty in CompPropertyGroup.Value)
						{
							BindingValueUnit Unit;
							if (CompProperty.Value is NOR.FrontDoor.Client.PropertyValue.DoublePropertyValue)
							{
								Unit = BindingValueUnits.Numeric;
							}
							else if(CompProperty.Value is NOR.FrontDoor.Client.PropertyValue.BoolPropertyValue)
							{
								Unit = BindingValueUnits.Boolean;
							}
							else if (CompProperty.Value is NOR.FrontDoor.Client.PropertyValue.LongPropertyValue)
							{
								Unit = BindingValueUnits.Numeric;
							}
							else
							{
								Unit = BindingValueUnits.Text;
							}

							PropertyGroupList.Add(new Tuple<string, string>(CompProperty.Key, BindingValueUnits.FetchUnitName(Unit)));
						}

						ComponentDict.Add(CompPropertyGroup.Key, PropertyGroupList);
					}

					_parent.ComponentsCache.Add(StrippedComp, ComponentDict);
				}
			}

			PopulateList();


			_parent.RefreshActionsAndTriggers();

			GrabStatus.Text = "";
			ConfigureButton.IsEnabled = true;
		}
	}
}
