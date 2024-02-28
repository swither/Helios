using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
using GadrocsWorkshop.Helios.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GadrocsWorkshop.Helios.Interfaces
{

	using GadrocsWorkshop.Helios.ComponentModel;
	using NLog;
	using NOR.FrontDoor;
	using NOR.FrontDoor.Client;
	using System.ComponentModel;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Runtime.Remoting.Contexts;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Runtime.Serialization;
	using System.Windows.Markup;
	using System.Windows.Threading;
	using System.Runtime.InteropServices.ComTypes;
	using System.Xml.Linq;
	using System.Reflection;
	using System.Windows.Media;
	using static GadrocsWorkshop.Helios.Interfaces.DCS.Common.NetworkTriggerValue;
	using NOR.FrontDoor.Client.PropertyValue;

	/// <summary>
	/// NOR Frontdoor interface
	/// </summary>
	[HeliosInterface(
		"Helios.NOR",                          // Helios internal type ID used in Profile XML, must never change
		"NOR",                 // human readable UI name for this interface
		typeof(NORInterfaceEditor),               // uses nor interface dialog for setup
		typeof(UniqueHeliosInterfaceFactory),     // can't be instantiated when specific other interfaces are present
		UniquenessKey = "Helios.NOR")]   // all other NOR interfaces exclude this interface




	
	internal class NORInterface : HeliosInterface
	{


		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

		FrontDoorClient FDClient = null;
		
		private readonly Dispatcher _dispatcher;

		//Configuration
		public string Address = "127.0.0.1:5555";
		public Dictionary<String, Dictionary<String, List<Tuple<string, string>>>> ComponentsCache;
		public List<Tuple<String, String>> ClickablesCache;

		public string TypeName = "Unknown";

		//Cache of existing components and clickables
		Dictionary<String, NOR.FrontDoor.Client.PropertyValue.IPropertyValue> Clickables;

		//States
		string PlayerPath = "";
		bool HookedUp = false;

		//Component data hooked up
		public Dictionary<String, Dictionary<String, HeliosTrigger>> BoundProperties;

		//Clickables hooked up
		public Dictionary<String, HeliosTrigger> BoundClickables;
		public Dictionary<String, HeliosAction> BoundClickablesActions;


		//Actions hooked up

		public NORInterface(string name)
	   : base(name)
		{
			_dispatcher = System.Windows.Application.Current.Dispatcher;

			/*
			//Actions - Add cached clickables here?
			HeliosAction TestAction = new HeliosAction(this, "", "", "NOR Test Action", "Test action for NOR plugin", "State", BindingValueUnits.Text);
			Actions.Add(TestAction);
			GearHandleAction = new HeliosAction(this, "", "", "Gear Handle", "Gear Handle Action");
			GearHandleAction.Execute += GearHandleAction_Execute;
			Actions.Add(GearHandleAction);
			

			//Triggers
			TestTrigger = new HeliosTrigger(this, "", "NOR Test Trigger", "changed", "test value", "Something something", BindingValueUnits.Degrees);
			GearHandleLight = new HeliosTrigger(this, "", "Gear Handle Light", "changed", "test value", "Something something", BindingValueUnits.Boolean);
			GearHandleState = new HeliosTrigger(this, "", "Gear Handle State", "changed", "test value", "Something something", BindingValueUnits.Numeric);
			Triggers.Add(TestTrigger);
			Triggers.Add(GearHandleLight);
			Triggers.Add(GearHandleState);

			//Values
			Values.Add(new HeliosValue(this, BindingValue.Empty, "", "Nor Test Value", "Used to test values in NOR plugin", "The value has been arbitrarily chosen", BindingValueUnits.Numeric));
			*/
		}


		private void GearHandleAction_Execute(object action, HeliosActionEventArgs e)
		{
			if (FDClient != null)
			{
				var Commands = new Dictionary<string, NOR.FrontDoor.Client.PropertyValue.IPropertyValue>
				{ 
					{ 
						"LGHandle", 
						new NOR.FrontDoor.Client.PropertyValue.StringPropertyValue(Clickables["LGHandle"].StringValue == "LGDown" ?  "LGUp" : "LGDown") 
					}
				};
				FDClient.SetClickableStates(Commands);
			}
		}

		public void RefreshActionsAndTriggers()
		{
			Triggers.Clear();
			Actions.Clear();

			foreach (var Clickable in ClickablesCache)
			{
				HeliosAction Action = new HeliosAction(this, "Clickables", Clickable.Item1, "Interact", "Clickable interaction", "New State Value", BindingValueUnits.FetchUnitByName(Clickable.Item2));
				Actions.Add(Action);

				HeliosTrigger Trigger = new HeliosTrigger(this, "Clickables", Clickable.Item1, "changed", "Clickable value changed", "New State Value", BindingValueUnits.FetchUnitByName(Clickable.Item2));
				Triggers.Add(Trigger);
			}

			foreach (var Component in ComponentsCache)
			{
				foreach (var State in Component.Value)
				{
					foreach (var Property in State.Value)
					{
						HeliosTrigger Trigger = new HeliosTrigger(this, $"{Component.Key}.{State.Key}", Property.Item1, "changed", "Property value changed", "New property Value", BindingValueUnits.FetchUnitByName(Property.Item2));
						Triggers.Add(Trigger);
					}
				}
			}
		}


		public override void ReadXml(XmlReader reader)
		{

			while (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.Name)
				{
					case "FrontDoorAddress":
						Address = reader.ReadElementContentAsString(); 
						break;

					case "TypeName":
						TypeName = reader.ReadElementContentAsString();
						break;

					case "Clickables":
						string encoded = reader.ReadElementContentAsString();

						MemoryStream inputStream = new MemoryStream(Convert.FromBase64String(encoded));
						IFormatter formatter = new BinaryFormatter();
						ClickablesCache = (List<Tuple<String, String>>)formatter.Deserialize(inputStream);
						break;

					case "Components":
						string encodedComp = reader.ReadElementContentAsString();

						MemoryStream inputStreamComp = new MemoryStream(Convert.FromBase64String(encodedComp));
						IFormatter formatterComp = new BinaryFormatter();
						ComponentsCache = (Dictionary<String, Dictionary<String, List<Tuple<string, string>>>>)formatterComp.Deserialize(inputStreamComp);
						break;

					default:
						string elementName = reader.Name;
						string discard = reader.ReadInnerXml();
						Logger.Warn(
							$"Ignored unsupported NOR Interface setting '{elementName}' with value '{discard}'");
						break;
				}
			}

			RefreshActionsAndTriggers();
		}

		public override void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("FrontDoorAddress", Address);
			writer.WriteElementString("TypeName", TypeName);

			MemoryStream outputStream = new MemoryStream();

			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(outputStream, ClickablesCache);
			byte[] bytes = outputStream.ToArray();

			string encoded = Convert.ToBase64String(bytes,	Base64FormattingOptions.InsertLineBreaks);
			writer.WriteStartElement("Clickables");
			writer.WriteCData(encoded);
			writer.WriteEndElement();

			outputStream = new MemoryStream(); ;
			formatter.Serialize(outputStream, ComponentsCache);
			bytes = outputStream.ToArray();
			encoded = Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
			writer.WriteStartElement("Components");
			writer.WriteCData(encoded);
			writer.WriteEndElement();
		}


		protected override void OnProfileChanged(HeliosProfile oldProfile)
		{
			base.OnProfileChanged(oldProfile);
			if (oldProfile != null)
			{
				oldProfile.ProfileStarted -= Profile_ProfileStarted;
				oldProfile.ProfileStopped -= Profile_ProfileStopped;
			}

			if (Profile != null)
			{
				Profile.ProfileStarted += Profile_ProfileStarted;
				Profile.ProfileStopped += Profile_ProfileStopped;
			}
		}

		private void Profile_ProfileStopped(object sender, EventArgs e)
		{
			if (FDClient != null)
			{
				//Remove front door client
				FDClient.Dispose();
			}
		}

		private async void Profile_ProfileStarted(object sender, EventArgs e)
		{
			//Create frontdoor client
			FDClient = new FrontDoorClient("127.0.0.1:5555");

			//Bound actions
			BoundClickablesActions = new Dictionary<string, HeliosAction>();
			foreach (var Binding in InputBindings)
			{
				if (Binding.Action.Device == "Clickables")
				{
					BoundClickablesActions[Binding.Action.Name] = (HeliosAction)Binding.Action;
				}
			}
			foreach (var Binding in BoundClickablesActions)
			{
				Binding.Value.Execute += ClickableExecute;
			}

			//Bounds triggers
			BoundClickables = new Dictionary<string, HeliosTrigger>();
			BoundProperties = new Dictionary<string, Dictionary<string, HeliosTrigger>>();
			foreach (var Binding in OutputBindings)
			{
				if (Binding.Trigger.Device == "Clickables")
				{
					BoundClickables[Binding.Trigger.Name] =  (HeliosTrigger)Binding.Trigger;
				}
				else
				{
					//public Dictionary<String, Dictionary<String, HeliosTrigger>> BoundProperties;
					if (!BoundProperties.ContainsKey(Binding.Trigger.Device))
					{
						BoundProperties.Add(Binding.Trigger.Device, new Dictionary<string, HeliosTrigger>());
					}
					BoundProperties[Binding.Trigger.Device].Add(Binding.Trigger.Name, (HeliosTrigger)Binding.Trigger);
				}
				
			}


			ProcessPlayerState(await FDClient.GetPlayerState());
			_ = FDClient.StreamPlayerEvents(OnPlayerStateReceived);

		}

		private void ClickableExecute(object action, HeliosActionEventArgs e)
		{
			if (FDClient != null)
			{
				IPropertyValue Value;

				if (((HeliosAction)action).Unit == BindingValueUnits.Numeric)
				{
					Value = new NOR.FrontDoor.Client.PropertyValue.DoublePropertyValue(e.Value.DoubleValue);
				}
				else
				{
					Value = new NOR.FrontDoor.Client.PropertyValue.StringPropertyValue(e.Value.StringValue);
				}

				var Commands = new Dictionary<string, NOR.FrontDoor.Client.PropertyValue.IPropertyValue>
				{
					{
						((HeliosAction)action).Name,
						Value
					}
				};
				FDClient.SetClickableStates(Commands);
			}
		}

		async Task SetupStreamsAsync()
		{

			//Grab all clickable states and then start streaming them for updates
			if (BoundClickables.Count > 0)
			{
				Clickables = new Dictionary<string, NOR.FrontDoor.Client.PropertyValue.IPropertyValue>();
				ProcessReceivedClickables(await FDClient.GetClickableStates());
				_ = FDClient.StreamClickableStates(OnClickableStatesReceived);


			}


			foreach (var BoundProperty in BoundProperties)
			{
				var StrippedPath = BoundProperty.Key.Substring(0, BoundProperty.Key.LastIndexOf('.'));
				var ComponentPath = PlayerPath + '.' + StrippedPath;
		
				ProcessProperties(StrippedPath, await FDClient.GetComponentProperties(ComponentPath));
				_ = FDClient.StreamComponent(ComponentPath, OnPropertiesReceived);
			}



			HookedUp = true;
		}

		void TearDownStreams()
		{
			if (!HookedUp)
			{
				return;
			}

			if (FDClient != null)
			{
				if (BoundClickables.Count > 0)
				{
					FDClient.UnStreamClickableStates();
				}

				foreach (var BoundProperty in BoundProperties)
				{
					var ComponentPath = PlayerPath + '.' + BoundProperty.Key;
					_ = FDClient.UnStreamComponent(ComponentPath);
				}

			}
			HookedUp = false;
		}


		private void ProcessReceivedClickables(Dictionary<string, Dictionary<string, NOR.FrontDoor.Client.PropertyValue.IPropertyValue>> Data)
		{
			
			foreach (var BoundClickable in BoundClickables)
			{
				if (!Data["Clickables"].ContainsKey(BoundClickable.Key))
				{
					continue;
				}

				var Value = Data["Clickables"][BoundClickable.Key];

				BindingValue bindValue;
				if (Value is NOR.FrontDoor.Client.PropertyValue.DoublePropertyValue)
				{
					bindValue = new BindingValue(((NOR.FrontDoor.Client.PropertyValue.DoublePropertyValue)Value).DoubleValue);
				}
				else
				{
					bindValue = new BindingValue(((NOR.FrontDoor.Client.PropertyValue.StringPropertyValue)Value).StringValue);
				}

				_dispatcher.Invoke(() => BoundClickable.Value.FireTrigger(bindValue),
				System.Windows.Threading.DispatcherPriority.Send);
			}
		}


		void ProcessPlayerState(Dictionary<string, Dictionary<string, NOR.FrontDoor.Client.PropertyValue.IPropertyValue>> Player)
		{
			bool Success = true;

			if (!Player.ContainsKey("Possessed"))
			{
				Success = false;

				//Unstream stuff
				TearDownStreams();
				return;
			}
			var Possessed = Player["Possessed"];

			if (!Possessed.ContainsKey("ActorPath"))
			{
				Success = false;
			}

			if (!Possessed.ContainsKey("TypeName"))
			{
				Success = false;
			}

			if (Possessed["TypeName"].StringValue != TypeName)
			{
				Success = false;
			}

			if (Success)
			{
				PlayerPath = Possessed["ActorPath"].StringValue; ;

				//Stream stuff
				_= SetupStreamsAsync();

			}
			else
			{
				//Unstream stuff
				TearDownStreams();
			}
		}

		void ProcessProperties(String Component, Dictionary<string, Dictionary<string, NOR.FrontDoor.Client.PropertyValue.IPropertyValue>> Properties)
		{
			foreach (var State in Properties)
			{
				var Combined = Component + "." +  State.Key;
				if (BoundProperties.ContainsKey(Combined))
				{
					var BoundComponent = BoundProperties[Combined];

					foreach (var KvP in BoundComponent)
					{
						var Value = State.Value[KvP.Key];

						BindingValue bindValue;
						if (Value is NOR.FrontDoor.Client.PropertyValue.DoublePropertyValue)
						{
							bindValue = new BindingValue(((NOR.FrontDoor.Client.PropertyValue.DoublePropertyValue)Value).DoubleValue);
						}
						else if (Value is NOR.FrontDoor.Client.PropertyValue.BoolPropertyValue)
						{
							bindValue = new BindingValue(((NOR.FrontDoor.Client.PropertyValue.BoolPropertyValue)Value).Value);
						}
						else if (Value is NOR.FrontDoor.Client.PropertyValue.LongPropertyValue)
						{
							bindValue = new BindingValue(((NOR.FrontDoor.Client.PropertyValue.LongPropertyValue)Value).LongValue);
						}
						else
						{
							bindValue = new BindingValue(((NOR.FrontDoor.Client.PropertyValue.StringPropertyValue)Value).StringValue);
						}

						_dispatcher.Invoke(() => KvP.Value.FireTrigger(bindValue),
						System.Windows.Threading.DispatcherPriority.Send);

					}
				}

			}


		}
	

		private Task OnPlayerStateReceived(StreamResult arg)
		{
			ProcessPlayerState(arg.Data);
			return Task.CompletedTask;
		}

		private Task OnClickableStatesReceived(StreamResult arg)
		{
			ProcessReceivedClickables(arg.Data);

			return Task.CompletedTask;
		}

		private Task OnPropertiesReceived(StreamResult arg)
		{
			String StrippedComp = arg.Component.Replace(PlayerPath + ".", "");

			ProcessProperties(StrippedComp, arg.Data);


			return Task.CompletedTask;
		}
	}
}
