//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Gauges.AH64D.CMWS
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows;
    using System.Windows.Media;

    [HeliosControl("Helios.AH64D.CMWS", "CMWS Display", "AH-64D", typeof(BackgroundImageRenderer))]
    public class CMWSDisplay : CompositeVisualWithBackgroundImage
    {
        private string _interfaceDeviceName = "CMWS";
        private string _font = "Helios Virtual Cockpit A-10C_ALQ_213";
        private static readonly Rect SCREEN_RECT = new Rect(0, 0, 1, 1);
        private Rect _scaledScreenRect = SCREEN_RECT;
        private CMWSThreatDisplay _display;

        public CMWSDisplay()
            : base("CMWS Display", new Size(1001, 350))
        {
            SupportedInterfaces = new[] { typeof(Interfaces.DCS.AH64D.AH64DInterface) };

            AddCMWSPart("Threat Display", new Point(651, 0), new Size(350, 350), _interfaceDeviceName, "CMWS Threat Display");
            AddTextDisplay("Line 1", new Point(20, 0), new Size(611, 175), _interfaceDeviceName, "Line 1", 96, "F  OUT", TextHorizontalAlignment.Left, "");
            AddTextDisplay("Line 2", new Point(20, 175), new Size(611, 175), _interfaceDeviceName, "Line 2", 96, "C  OUT", TextHorizontalAlignment.Left, "");
        }
        private void AddTextDisplay(string name, Point posn, Size size,
    string interfaceDeviceName, string interfaceElementName, double baseFontsize, string testDisp, TextHorizontalAlignment hTextAlign, string devDictionary)
        {
            TextDisplay display = AddTextDisplay(
                name: name,
                posn: posn,
                size: size,
                font: _font,
                baseFontsize: baseFontsize,
                horizontalAlignment: hTextAlign,
                verticalAligment: TextVerticalAlignment.Center,
                testTextDisplay: testDisp,
                textColor: Color.FromArgb(0xcc, 0x50, 0xc3, 0x39),
                backgroundColor: Color.FromArgb(0xff, 0x04, 0x2a, 0x00),
                useBackground: false,
                interfaceDeviceName: interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                textDisplayDictionary: devDictionary
                );
        }
        private void AddCMWSPart(string name, Point pos, Size size, string interfaceDevice, string interfaceElement)
        {
            _display = new CMWSThreatDisplay
            {
                Top = pos.Y,
                Left = pos.X,
                Height = size.Height,
                Width = size.Width,
                Name = GetComponentName(name)
            };
            Children.Add(_display);
            // Note:  we have the actions against the new CMWSThreatDisplay but to expose those
            // actions in the interface, we copy the actions to the Parent.  This is a new 
            // HeliosActionCollection with the keys equal to the new ActionIDs, however the original
            // HeliosActionCollection which is on the child part will have the original keys, even though
            // we might have changed the values of the ActionIDs.  This has the result that autobinding
            // in CompositeVisual (OnProfileChanged) might not be able to find the actions when doing
            // the "ContainsKey()" for the action.
            // This is why the _display.Name is in the deviceActionName of the AddDefaultInputBinding
            // and *MUST* match the BindingValue device parameter for the gauge being added.

            //foreach (IBindingTrigger trigger in _display.Triggers)
            //{
            //    AddTrigger(trigger, trigger.Name);
            //}
            foreach (IBindingAction action in _display.Actions)
            {
                if(action.Name != "hidden")
                {

                AddAction(action, _display.Name);
                //Create the automatic input bindings for the sub component
                AddDefaultInputBinding(
                   childName: _display.Name,
                   deviceActionName: _display.Name +"." + action.ActionVerb + "." + action.Name,
                   interfaceTriggerName: interfaceDevice + "." + action.Name + ".changed"
                   );
                }

            }
            //_display.Actions.Clear();
        }
        public override string DefaultBackgroundImage
        {
            get { return "{Helios}/Gauges/AH-64D/CMWS/cmws_background.xaml"; }
        }
        public override bool HitTest(Point location)
        {
            if (_scaledScreenRect.Contains(location))
            {
                return false;
            }

            return true;
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
    }
}
