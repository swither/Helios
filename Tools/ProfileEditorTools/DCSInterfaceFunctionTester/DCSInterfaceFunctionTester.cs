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
                                PlaceControl<Controls.ImageDecoration>(panel, dcsFunction);

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
                            PlaceControl<Controls.RotaryEncoder>(panel, dcsFunction);
                        AddBinding(rotaryEncoderControl, "encoder incremented", rotaryEncoderFunction, "increment",
                            true);
                        AddBinding(rotaryEncoderControl, "encoder decremented", rotaryEncoderFunction, "decrement",
                            true);

                        break;
                    }
                    case Axis axisFunction:
                    {
                        Controls.Potentiometer potentiometerControl =
                            PlaceControl<Controls.Potentiometer>(panel, dcsFunction);
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
                            PlaceControl<Controls.Indicator>(panel, dcsFunction);
                        indicatorControl.Text = "";
                        indicatorControl.OnImage = "{Helios}/Images/Indicators/green-light-on.png";
                        indicatorControl.OffImage = "{Helios}/Images/Indicators/green-light-off.png";
                        AddBinding(flagValueFunction, "changed", indicatorControl, "set indicator",
                            true);
                        break;
                    }
                    case ScaledNetworkValue _:
                    case NetworkValue _:
                    case NetworkTriggerValue _:
                    {
                        ConfigureAsNumberDisplay(panel, dcsFunction);
                        break;
                    }
                    case NetworkTrigger networkTriggerFunction:
                    {
                        Controls.Indicator indicatorControl =
                            PlaceControl<Controls.Indicator>(panel, dcsFunction);
                        indicatorControl.Text = "";
                        indicatorControl.OnImage = "{Helios}/Images/Indicators/green-light-on.png";
                        indicatorControl.OffImage = "{Helios}/Images/Indicators/green-light-off.png";
                        HeliosBinding binding = AddBinding(networkTriggerFunction, "received", indicatorControl,
                            // NOTE: trailing space
                            "toggle indicator ", 
                            true);
                        binding.ValueSource = BindingValueSources.TriggerValue;
                        break;
                    }
                    default:
                    {
                        // unsupported
                        PlaceControl<Controls.ImageDecoration>(panel, dcsFunction);
                        break;
                    }
                }
            }

            // place as many panel buttons as we can in order at the top
            ArrangeButtons();

            // connect all the show/hide actions
            Panel.ConnectButtons(Panels.Values.ToList());
        }

        private void ConfigureAsNumberDisplay(Panel panel, DCSFunction dcsFunction)
        {
            Controls.NumericTextDisplay numericTextDisplay =
                PlaceControl<Controls.NumericTextDisplay>(panel, dcsFunction);
            numericTextDisplay.Precision = 3;
            numericTextDisplay.TextFormat.FontSize = 20;
            numericTextDisplay.TextFormat.HorizontalAlignment = TextHorizontalAlignment.Right;
            numericTextDisplay.TextFormat.PaddingRight = 0.02;

            HeliosBinding binding = AddBinding(dcsFunction, "changed", numericTextDisplay,
                "set Number",
                true);
            binding.ValueSource = BindingValueSources.TriggerValue;
        }

        private void ArrangeButtons()
        {
            int buttonRow = 0;
            int buttonColumn = 0;
            foreach (Controls.PushButton selectButton in Panels.OrderBy(pair => pair.Key)
                .SelectMany(pair => pair.Value.SelectButtons, (p, s) => s))
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
            Controls.RotarySwitch rotarySwitchControl;
            bool foundNamed = false;

            // check if we will draw labels
            foreach (SwitchPosition switchFunctionPosition in switchFunction.Positions)
            {
                if (!foundNamed && !switchFunctionPosition.Name.StartsWith("Position"))
                {
                    foundNamed = true;
                }
            }

            // place control accordingly
            if (switchFunction.Positions.Count() <= 8 && foundNamed)
            {
                rotarySwitchControl = PlaceControl<Controls.RotarySwitch>(panel, switchFunction, 2, 0.75d);
                rotarySwitchControl.LabelFormat.FontSize = 8;
            }
            else
            {
                // not enough space for these or they just all say "Position..." anyway
                rotarySwitchControl = PlaceControl<Controls.RotarySwitch>(panel, switchFunction);
                rotarySwitchControl.DrawLabels = false;
            }


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

            AddBinding(switchFunction, "changed", rotarySwitchControl, "set position", true);
            AddBinding(rotarySwitchControl, "position changed", switchFunction, "set", true);
        }

        private void ConfigureAsThreeWayToggle(Panel panel, Switch switchFunction)
        {
            Controls.ThreeWayToggleSwitch threeWayToggleSwitchControl =
                PlaceControl<Controls.ThreeWayToggleSwitch>(panel, switchFunction);
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
                PlaceControl<Controls.ToggleSwitch>(panel, switchFunction);
            toggleSwitchControl.Left += toggleSwitchControl.Width / 4;
            toggleSwitchControl.Width /= 2;
            AddBinding(switchFunction, "changed", toggleSwitchControl, "set position", true);
            AddBinding(toggleSwitchControl, "position changed", switchFunction, "set", true);
        }

        private void ConfigureAsPushButton(Panel panel, PushButton pushButtonFunction)
        {
            Controls.PushButton pushButtonControl =
                PlaceControl<Controls.PushButton>(panel, pushButtonFunction);
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
                PlaceControl<Controls.TextDecoration>(panel, dcsFunction);
            textDecorationControl.FontColor = Color.FromRgb(255, 0, 0);
            textDecorationControl.Text = "Needs Code";
            textDecorationControl.Format.FontSize = 20;
            textDecorationControl.Format.HorizontalAlignment = TextHorizontalAlignment.Center;
            textDecorationControl.Format.VerticalAlignment = TextVerticalAlignment.Center;
        }

        private void SelectAndConfigureMonitor()
        {
            TargetMonitor = SelectMonitor();
            TargetMonitor.AlwaysOnTop = true;
            TargetMonitor.FillBackground = true;
        }

        private Monitor SelectMonitor()
        {
            return Profile.Monitors.OrderByDescending(m => m.Right).First();
        }

        private TType PlaceControl<TType>(Panel panel, DCSFunction dcsFunction, int columns = 1, double scale = 1d) where TType : HeliosVisual
        {
            panel.PlaceControlAndLabel(dcsFunction, columns, out double left, out double top, out double width, out double height);
            TType control = Activator.CreateInstance<TType>();

            control.Name = $"{dcsFunction.DeviceName} {dcsFunction.Name}";
            control.Left = left + width * (1d - scale) / 2d;
            control.Top = top + height * (1d - scale) / 2d; 
            control.Width = width * scale;
            control.Height = height * scale;
            panel.Container.Children.Insert(0, control);

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
            Profile?.Interfaces.FirstOrDefault(i => i is DCSInterface) != null &&
            !SelectMonitor().Children.Any();

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