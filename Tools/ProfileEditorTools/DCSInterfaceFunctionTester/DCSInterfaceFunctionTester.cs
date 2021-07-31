// Copyright 2021 Ammo Goettsch
// 
// ProfileEditorTools is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ProfileEditorTools is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
using GadrocsWorkshop.Helios.Tools;
using GadrocsWorkshop.Helios.Tools.Capabilities;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.ProfileEditorTools.DCSInterfaceFunctionTester
{
    [HeliosTool]
    internal class DCSInterfaceFunctionTester : ProfileTool, IMenuSectionFactory
    {
        internal const double CONTROL_HEIGHT = 100;
        internal const double CONTROL_WIDTH = 100;
        internal const double LABEL_FONT_SIZE = 14;
        internal const double LABEL_HEIGHT = 100;

        private void Excecute()
        {
            SelectAndConfigureMonitor();

            DCSInterface dcs = Profile.Interfaces.OfType<DCSInterface>().First();
            foreach (DCSFunction dcsFunction in dcs.Functions.OfType<DCSFunction>())
            {
                Panel panel = GetOrCreatePanel(dcsFunction.DeviceName);
                if (dcsFunction.Unimplemented)
                {
                    ConfigureAsNeedsCode(panel, dcsFunction);
                    continue;
                }

                switch (dcsFunction)
                {
                    case PushButton pushButtonFunction:
                        ConfigureAsPushButton(panel, pushButtonFunction);
                        break;
                    case Switch switchFunction:
                        int numPositions = switchFunction.Positions.Count();
                        switch (numPositions)
                        {
                            case 0:
                            case 1:
                            {
                                // can't be implemented
                                PlaceFullSize<Controls.ImageDecoration>(panel, dcsFunction);

                                break;
                            }
                            case 2:
                            {
                                // 2-position toggle switch
                                ConfigureAsToggleSwitch(panel, switchFunction);

                                break;
                            }
                            case 3:
                            {
                                // 3-position toggle switch
                                ConfigureAsThreeWayToggle(panel, switchFunction);

                                break;
                            }
                            default:
                            {
                                // all other amounts of switch position: rotary switch
                                ConfigureAsRotarySwitch(panel, switchFunction, numPositions);

                                break;
                            }
                        }

                        break;
                    case RotaryEncoder rotaryEncoderFunction:
                    {
                        Controls.RotaryEncoder rotaryEncoderControl =
                            PlaceFullSize<Controls.RotaryEncoder>(panel, dcsFunction);
                        AddBinding(rotaryEncoderControl, "encoder incremented", rotaryEncoderFunction, "increment",
                            true);
                        AddBinding(rotaryEncoderControl, "encoder decremented", rotaryEncoderFunction, "decrement",
                            true);

                        break;
                    }
                    case Axis axisFunction:
                    {
                        Controls.Potentiometer potentiometerControl =
                            PlaceFullSize<Controls.Potentiometer>(panel, dcsFunction);
                        potentiometerControl.MinValue = axisFunction.ArgumentMin;
                        potentiometerControl.MaxValue = axisFunction.ArgumentMax;
                        potentiometerControl.StepValue = axisFunction.StepValue;
                        AddBinding(potentiometerControl, "value changed", axisFunction, "set",
                            true);
                        AddBinding(axisFunction, "changed", potentiometerControl, "set value",
                            true);
                        break;
                    }
                    case FlagValue flagValueFunction:
                    {
                        Controls.Indicator indicatorControl =
                            PlaceFullSize<Controls.Indicator>(panel, dcsFunction);
                        indicatorControl.Text = "";
                        indicatorControl.OnImage = "{Helios}/Images/Indicators/green-light-on.png";
                        indicatorControl.OffImage = "{Helios}/Images/Indicators/green-light-off.png";
                        AddBinding(flagValueFunction, "changed", indicatorControl, "set indicator",
                            true);
                        break;
                    }
                    case ScaledNetworkValue scaledNetworkValueFunction:
                    {
                        Controls.TextDisplay textDisplayControl =
                            PlaceFullSize<Controls.TextDisplay>(panel, dcsFunction);
                        HeliosBinding binding = AddBinding(scaledNetworkValueFunction, "changed", textDisplayControl,
                            "set TextDisplay",
                            true);
                        binding.ValueSource = BindingValueSources.LuaScript;
                        binding.Value = "return tostring(TriggerValue)";
                        break;
                    }
                    case NetworkValue networkValueFunction:
                    {
                        Controls.TextDisplay textDisplayControl =
                            PlaceFullSize<Controls.TextDisplay>(panel, dcsFunction);
                        HeliosBinding binding = AddBinding(networkValueFunction, "changed", textDisplayControl,
                            "set TextDisplay",
                            true);
                        binding.ValueSource = BindingValueSources.LuaScript;
                        binding.Value = "return tostring(TriggerValue)";
                        break;
                    }
                    case NetworkTriggerValue networkTriggerValueFunction:
                    {
                        Controls.TextDisplay textDisplayControl =
                            PlaceFullSize<Controls.TextDisplay>(panel, dcsFunction);
                        HeliosBinding binding = AddBinding(networkTriggerValueFunction, "changed", textDisplayControl,
                            "set TextDisplay",
                            true);
                        binding.ValueSource = BindingValueSources.LuaScript;
                        binding.Value = "return tostring(TriggerValue)";
                        break;
                    }
                    default:
                    {
                        // unsupported
                        PlaceFullSize<Controls.ImageDecoration>(panel, dcsFunction);
                        break;
                    }
                }
            }

            ArrangeButtons();
        }

        private void ArrangeButtons()
        {
            int buttonRow = 0;
            int buttonColumn = 0;
            foreach (Controls.PushButton selectButton in Panels.OrderBy(pair => pair.Key).SelectMany(pair => pair.Value.SelectButtons, (p, s) => s))
            {
                if ((buttonColumn + 1) * Panel.SELECT_BUTTON_WIDTH > TargetMonitor.Width)
                {
                    // row full
                    buttonColumn = 0;
                    buttonRow++;
                }

                if ((buttonRow + 1) * Panel.SELECT_BUTTON_HEIGHT > Panel.TOP_SPACE)
                {
                    // out of space for buttons
                    break;
                }

                // now place
                selectButton.Left = buttonColumn * Panel.SELECT_BUTTON_WIDTH;
                selectButton.Top = buttonRow * Panel.SELECT_BUTTON_HEIGHT;
                TargetMonitor.Children.Add(selectButton);
                buttonColumn++;
            }
        }

        private void ConfigureAsRotarySwitch(Panel panel, Switch switchFunction, int numPositions)
        {
            Controls.RotarySwitch rotarySwitchControl =
                PlaceFullSize<Controls.RotarySwitch>(panel, switchFunction);

            // remove pre-created switch positions
            rotarySwitchControl.Positions.RemoveAt(rotarySwitchControl.Positions.Count - 1);
            rotarySwitchControl.Positions.RemoveAt(rotarySwitchControl.Positions.Count - 1);


            // create ours
            int i = 0;
            foreach (SwitchPosition switchFunctionPosition in switchFunction.Positions)
            {
                rotarySwitchControl.Positions.Add(new Controls.RotarySwitchPosition(
                    rotarySwitchControl, i + 1,
                    switchFunctionPosition.Name, 270.0 * i / numPositions));
                i++;
            }

            // not enough space for these
            rotarySwitchControl.DrawLabels = false;


            AddBinding(switchFunction, "changed", rotarySwitchControl, "set position", true);
            AddBinding(rotarySwitchControl, "position changed", switchFunction, "set", true);
        }

        private void ConfigureAsThreeWayToggle(Panel panel, Switch switchFunction)
        {
            Controls.ThreeWayToggleSwitch threeWayToggleSwitchControl =
                PlaceFullSize<Controls.ThreeWayToggleSwitch>(panel, switchFunction);
            threeWayToggleSwitchControl.Left += threeWayToggleSwitchControl.Width / 4;
            threeWayToggleSwitchControl.Width /= 2;
            AddBinding(switchFunction, "changed", threeWayToggleSwitchControl, "set position",
                true);
            AddBinding(threeWayToggleSwitchControl, "position changed", switchFunction, "set",
                true);
        }

        private void ConfigureAsToggleSwitch(Panel panel, Switch switchFunction)
        {
            Controls.ToggleSwitch toggleSwitchControl =
                PlaceFullSize<Controls.ToggleSwitch>(panel, switchFunction);
            toggleSwitchControl.Left += toggleSwitchControl.Width / 4;
            toggleSwitchControl.Width /= 2;
            AddBinding(switchFunction, "changed", toggleSwitchControl, "set position", true);
            AddBinding(toggleSwitchControl, "position changed", switchFunction, "set", true);
        }

        private void ConfigureAsPushButton(Panel panel, PushButton pushButtonFunction)
        {
            Controls.PushButton pushButtonControl =
                PlaceFullSize<Controls.PushButton>(panel, pushButtonFunction);
            // WARNING: note the trailing spaces on the action names due to Helios' appending a space and an empty name
            AddBinding(pushButtonFunction, "pushed", pushButtonControl, "push ", true);
            AddBinding(pushButtonFunction, "released", pushButtonControl, "release ", true);
            AddBinding(pushButtonControl, "pushed", pushButtonFunction, "push", true);
            AddBinding(pushButtonControl, "released", pushButtonFunction, "release", true);
        }

        private void ConfigureAsNeedsCode(Panel panel, DCSFunction dcsFunction)
        {
            // this function is marked as missing code or implementation from the generator, so call attention to it
            Controls.TextDecoration textDecorationControl =
                PlaceFullSize<Controls.TextDecoration>(panel, dcsFunction);
            textDecorationControl.FontColor = Color.FromRgb(255, 0, 0);
            textDecorationControl.Text = "Needs Code";
            textDecorationControl.Format.FontSize = 20;
            textDecorationControl.Format.HorizontalAlignment = TextHorizontalAlignment.Center;
            textDecorationControl.Format.VerticalAlignment = TextVerticalAlignment.Center;
        }

        private void SelectAndConfigureMonitor()
        {
            TargetMonitor = Profile.Monitors.OrderByDescending(m => m.Right).First();
            TargetMonitor.AlwaysOnTop = true;
            TargetMonitor.FillBackground = true;
        }

        private TType PlaceFullSize<TType>(Panel panel, DCSFunction dcsFunction) where TType : HeliosVisual
        {
            panel.PlaceControlAndLabel(dcsFunction, out double left, out double top, out double width,
                out double height, out bool addedNewPanel);
            TType control = Activator.CreateInstance<TType>();

            control.Name = $"{dcsFunction.DeviceName} {dcsFunction.Name}";
            control.Left = left;
            control.Top = top;
            control.Width = width;
            control.Height = height;
            panel.Container.Children.Insert(0, control);

            // also add new panels to button handlers
            if (addedNewPanel)
            {
                // hook up hide actions for new panel
                foreach (Controls.PushButton pushButton in Panels.Values.SelectMany(p => p.SelectButtons, (p, b) => b))
                {
                    AddSetHiddenBinding(pushButton, "pushed", panel.Container, "true");
                }

                // hook up show/hide actions for new button
                AddSetHiddenBinding(panel.SelectButton, "pushed", panel.Container, "false");
                foreach (Controls.HeliosPanel existingPanel in Panels.Values.SelectMany(p => p.Containers, (p, c) => c))
                {
                    AddSetHiddenBinding(panel.SelectButton, "pushed", existingPanel, "true");
                }
            }
            return control;
        }

        private Panel GetOrCreatePanel(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                deviceName = "Unnamed Device";
            }

            if (Panels.TryGetValue(deviceName, out Panel panel))
            {
                return panel;
            }

            panel = new Panel(TargetMonitor, deviceName);
            Panels.Add(deviceName, panel);
            return panel;
        }

        private HeliosBinding AddBinding(NetworkFunction source, string triggerVerb, HeliosVisual target,
            string actionName,
            bool bypassCascadingTriggers)
        {
            HeliosBinding binding = new HeliosBinding(
                source.Triggers.First(t => t.TriggerVerb == triggerVerb),
                target.Actions.First(a => a.ActionName == actionName))
            {
                BypassCascadingTriggers = bypassCascadingTriggers,
                ValueSource = BindingValueSources.TriggerValue
            };
            target.InputBindings.Add(binding);
            source.SourceInterface.OutputBindings.Add(binding);
            return binding;
        }

        private HeliosBinding AddBinding(HeliosVisual source, string triggerName, NetworkFunction target,
            string actionVerb,
            bool bypassCascadingTriggers)
        {
            HeliosBinding binding = new HeliosBinding(
                source.Triggers.First(t => t.TriggerName == triggerName),
                target.Actions.First(a => a.ActionVerb == actionVerb))
            {
                BypassCascadingTriggers = bypassCascadingTriggers,
                ValueSource = BindingValueSources.TriggerValue
            };
            target.SourceInterface.InputBindings.Add(binding);
            source.OutputBindings.Add(binding);
            return binding;
        }

        private HeliosBinding AddSetHiddenBinding(HeliosVisual source, string triggerName, HeliosVisual target,
            string actionValue)
        {
            HeliosBinding binding = new HeliosBinding(
                source.Triggers.First(t => t.TriggerName == triggerName),
                target.Actions.First(a => a.ActionName == "set hidden"))
            {
                ValueSource = BindingValueSources.StaticValue,
                Value = actionValue
            };
            target.InputBindings.Add(binding);
            source.OutputBindings.Add(binding);
            return binding;
        }

        #region Overrides

        public override void Close(HeliosProfile oldProfile)
        {
            base.Close(oldProfile);
            Panels.Clear();
            Row = 0;
            Column = 0;
        }

        public override bool CanStart =>
            base.CanStart &&
            Profile?.Interfaces.FirstOrDefault(i => i is DCSInterface) != null;

        #endregion

        #region IMenuSectionFactory

        public MenuSectionModel CreateMenuSection()
        {
            return new MenuSectionModel("DCS Interface Test", new List<MenuItemModel>
            {
                new MenuItemModel("Generate DCS Function Test Profile", new Windows.RelayCommand(
                    parameter => { Excecute(); },
                    parameter => CanStart))
            });
        }

        #endregion

        #region Properties

        public int Row { get; set; }

        public int Column { get; set; }

        internal Dictionary<string, Panel> Panels { get; } = new Dictionary<string, Panel>();

        internal Monitor TargetMonitor { get; private set; }

        #endregion
    }
}