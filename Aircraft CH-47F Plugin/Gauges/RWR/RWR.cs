﻿//  Copyright 2014 Craig Courtney
//  Copyright 2022 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Gauges.CH47F.RWR
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;
    using System.Globalization;
    using System.Xml.Linq;
    using System.Drawing.Imaging;
    using System.Windows.Media.Media3D;

    [HeliosControl("Helios.CH47F.RWR", "RADAR Warning Receiver", "CH-47F Chinook", typeof(BackgroundImageRenderer), HeliosControlFlags.None)]
    public class RWR : CompositeVisualWithBackgroundImage
    {
        private static readonly Rect SCREEN_RECT = new Rect(32, 32, 436, 436);
        private Rect _scaledScreenRect = SCREEN_RECT;
        private string _interfaceDevice = "";
        private HeliosPanel _frameGlassPanel;
        private HeliosPanel _frameBezelPanel;
        private HeliosPanel _reticlePanel;
        private bool _includeViewport = true;
        private string _vpName = "";
        private const string PANEL_IMAGE = "{CH-47F}/Gauges/RWR/Images/RWR_Bezel.png";
        public const double GLASS_REFLECTION_OPACITY_DEFAULT = 0.10d;
        private double _glassReflectionOpacity = GLASS_REFLECTION_OPACITY_DEFAULT;
        private readonly string _imagePath = "{CH-47F}/Gauges/RWR/Images";


        public RWR()
            : base("RWR", new Size(500, 500))
        {
            SupportedInterfaces = new[] { typeof(Interfaces.DCS.CH47F.CH47FInterface) };
            _interfaceDevice = "Canted Console";
            _vpName = "CH47F_RWR";

            if (_vpName != "" && _includeViewport) AddViewport(_vpName);
            _frameGlassPanel = AddPanel("RWR Glass", new Point(SCREEN_RECT.Left, SCREEN_RECT.Top), new Size(SCREEN_RECT.Width, SCREEN_RECT.Height), "{CH-47F}/Gauges/RWR/Images/RWR_glass.png", _interfaceDevice);
            _frameGlassPanel.Opacity = _glassReflectionOpacity;
            _frameGlassPanel.DrawBorder = false;
            _frameGlassPanel.FillBackground = false;

            _reticlePanel = AddPanel("RWR Reticle", new Point(SCREEN_RECT.Left, SCREEN_RECT.Top), new Size(SCREEN_RECT.Width, SCREEN_RECT.Height), "{CH-47F}/Gauges/RWR/Images/RWR_Rings.xaml", _interfaceDevice);
            _reticlePanel.Opacity = 1d;
            _reticlePanel.DrawBorder = false;
            _reticlePanel.FillBackground = false;

            _frameBezelPanel = AddPanel("RWR Frame", new Point(Left, Top), NativeSize, PANEL_IMAGE, _interfaceDevice);
            _frameBezelPanel.Opacity = 1d;
            _frameBezelPanel.FillBackground = false;
            _frameBezelPanel.DrawBorder = false;

            AddEncoder("RWR Brilliance Knob", new Point(388d, 15d), new Size(112d, 112d), "RWR Brilliance Knob");
            AddTwoWayToggle("Day/Night", new Point(437d, 391d), new Size(47d, 64d), _interfaceDevice, "RWR Day/Night Switch", "RWR_DAY_SW", ToggleSwitchPosition.Two);
        }
        public string ViewportName
        {
            get => _vpName;
            set
            {
                if (_vpName != value)
                {
                    if (_vpName == "")
                    {
                        AddViewport(value);
                        OnDisplayUpdate();
                    }
                    else if (value != "")
                    {
                        foreach (HeliosVisual visual in this.Children)
                        {
                            if (visual.TypeIdentifier == "Helios.Base.ViewportExtent")
                            {
                                Controls.Special.ViewportExtent viewportExtent = visual as Controls.Special.ViewportExtent;
                                viewportExtent.ViewportName = value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        RemoveViewport(value);
                    }
                    OnPropertyChanged("ViewportName", _vpName, value, false);
                    _vpName = value;
                }
            }
        }
        public bool RequiresPatches
        {
            get => _vpName != "" ? true : false;
            set => _ = value;
        }
        public double GlassReflectionOpacity
        {
            get
            {
                return _glassReflectionOpacity;
            }
            set
            {
                double oldValue = _glassReflectionOpacity;
                if (value != oldValue)
                {
                    _glassReflectionOpacity = value;
                    OnPropertyChanged("GlassReflectionOpacity", oldValue, value, true);
                    _frameGlassPanel.IsHidden = _glassReflectionOpacity == 0d ? true : false;
                    _frameGlassPanel.Opacity = _glassReflectionOpacity; 

                }
            }
        }
        protected HeliosPanel AddPanel(string name, Point posn, Size size, string background, string interfaceDevice)
        {
            HeliosPanel panel = AddPanel
                (
                name: name,
                posn: posn,
                size: size,
                background: background
                );
            // in this instance, we want to all the panels to be hide-able so the actions need to be added
            IBindingAction panelAction = panel.Actions["toggle.hidden"];
            panelAction.Device = $"{Name}_{name}";
            panelAction.Name = "hidden";
            if (!Actions.ContainsKey(panel.Actions.GetKeyForItem(panelAction)))
            {
                Actions.Add(panelAction);
                //string addedKey = Actions.GetKeyForItem(panelAction);
            }
            panelAction = panel.Actions["set.hidden"];
            panelAction.Device = $"{Name}_{name}";
            panelAction.Name = "hidden";
            if (!Actions.ContainsKey(panel.Actions.GetKeyForItem(panelAction)))
            {
                Actions.Add(panelAction);
                //string addedKey = Actions.GetKeyForItem(panelAction);
            }
            return panel;
        }
        private void AddViewport(string name)
        {
            Rect vpRect = SCREEN_RECT;
            vpRect.Scale(Width / NativeSize.Width, Height / NativeSize.Height);
            TextFormat tf = new TextFormat()
            {
                FontStyle = FontStyles.Normal,
                FontWeight = FontWeights.Normal,
                FontSize = 1.2,
                FontFamily = ConfigManager.FontManager.GetFontFamilyByName("Franklin Gothic"),
                ConfiguredFontSize = 1.2,
                HorizontalAlignment = TextHorizontalAlignment.Center,
                VerticalAlignment = TextVerticalAlignment.Center
            };
            Children.Add(new Helios.Controls.Special.ViewportExtent
            {
                FillBackground = true,
                BackgroundColor = Color.FromArgb(128, 128, 32, 64),
                FontColor = Color.FromArgb(255, 255, 255, 255),
                ViewportName = name,
                TextFormat = tf,
                Left = vpRect.Left,
                Top = vpRect.Top,
                Width = vpRect.Width,
                Height = vpRect.Height
            });
        }
        private void RemoveViewport(string name)
        {
            _includeViewport = false;
            foreach (HeliosVisual visual in this.Children)
            {
                if (visual.TypeIdentifier == "Helios.Base.ViewportExtent")
                {
                    Children.Remove(visual);
                    break;
                }
            }
        }
        private void AddEncoder(string name, Point posn, Size size, string interfaceElementName)
        {

            RotaryEncoder knob = new RotaryEncoder
            {
                Name = name,
                KnobImage = $"{_imagePath}/RWR_BRIL_Knob.png",
                StepValue = 0.1d,
                RotationStep = 5d,
                Top = posn.Y,
                Left = posn.X,
                Width = size.Width,
                Height = size.Height,
                ClickType = RotaryClickType.Swipe
            };

            Children.Add(knob);
            foreach (IBindingTrigger trigger in knob.Triggers)
            {
                AddTrigger(trigger, name);
            }

            AddAction(knob.Actions["set.value"], name);

            AddDefaultOutputBinding(
                childName: name,
                deviceTriggerName: "encoder.incremented",
                interfaceActionName: _interfaceDevice + ".increment." + interfaceElementName
            );
            AddDefaultOutputBinding(
                childName: name,
                deviceTriggerName: "encoder.decremented",
                interfaceActionName: _interfaceDevice + ".decrement." + interfaceElementName
                );

            AddDefaultInputBinding(
                childName: name,
                interfaceTriggerName: $"{_interfaceDevice}.{name}.changed",
                deviceActionName: "set.value"
                );
        }
        private void AddTwoWayToggle(string name, Point posn, Size size, string interfaceDeviceName, string interfaceElementName, string imageName, ToggleSwitchPosition ToggleDefaultPosition)
        {
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: posn,
                size: size,
                defaultPosition: ToggleDefaultPosition,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: $"{_imagePath}/{imageName}_Up.png",
                positionTwoImage: $"{_imagePath}/{imageName}_Down.png",
                horizontal: false,
                clickType: LinearClickType.Swipe,
                interfaceDeviceName: _interfaceDevice,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
        }
        private string ComponentName(string name)
        {
            return $"{Name}_{name}";
        }
        private new void AddTrigger(IBindingTrigger trigger, string name)
        {
            trigger.Device = ComponentName(name);
            if (!Triggers.ContainsKey(Triggers.GetKeyForItem(trigger))) Triggers.Add(trigger);

        }
        private new void AddAction(IBindingAction action, string name)
        {
            action.Device = ComponentName(name);
            if (!Actions.ContainsKey(Actions.GetKeyForItem(action))) Actions.Add(action);
        }

        public override string DefaultBackgroundImage => PANEL_IMAGE;

        protected override void OnBackgroundImageChange()
        {
            _frameBezelPanel.BackgroundImage = BackgroundImageIsCustomized ? null : PANEL_IMAGE;
        }
        public override bool HitTest(Point location)
        {
            if (_scaledScreenRect.Contains(location))
            {
                return false;
            }

            return false;
        }
        public override void MouseDown(Point location)
        {
            // No-Op
        }
        public override void MouseDrag(Point location)
        {
            // No-Op
        }
        public override void MouseUp(Point location)
        {
            // No-Op
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            if (_includeViewport)
            {
                writer.WriteElementString("EmbeddedViewportName", _vpName);
            }
            else
            {
                writer.WriteElementString("EmbeddedViewportName", "");
            }
            if (_glassReflectionOpacity > 0d)
            {
                writer.WriteElementString("GlassReflectionOpacity", GlassReflectionOpacity.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            _includeViewport = true;
            if (reader.Name.Equals("EmbeddedViewportName"))
            {
                _vpName = reader.ReadElementString("EmbeddedViewportName");
                if (_vpName == "")
                {
                    _includeViewport = false;
                    RemoveViewport("");
                }
            }

            GlassReflectionOpacity = reader.Name.Equals("GlassReflectionOpacity") ? double.Parse(reader.ReadElementString("GlassReflectionOpacity"), CultureInfo.InvariantCulture) : 0d;
        }
    }
}
