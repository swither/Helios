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
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;

namespace GadrocsWorkshop.Helios.ProfileEditorTools.DCSInterfaceFunctionTester
{
    internal class Panel
    {
        internal const double SELECT_BUTTON_HEIGHT = 33;

        internal const double SELECT_BUTTON_WIDTH = 75;
        internal const double TOP_SPACE = 166;

        private const double ROW_HEIGHT =
            DCSInterfaceFunctionTester.CONTROL_HEIGHT + DCSInterfaceFunctionTester.LABEL_HEIGHT;

        public Panel(Monitor targetMonitor, string deviceName)
        {
            TargetMonitor = targetMonitor;
            DeviceName = deviceName;

            // create a full screen panel with some reserved space at the top
            // create a button for it, we will place it later after we have all of them
            AddPanel();
        }

        public void PlaceControlAndLabel(DCSFunction dcsFunction, int columns, out double left, out double top, out double width,
            out double height)
        {
            left = DCSInterfaceFunctionTester.CONTROL_WIDTH * (Column + (columns - 1d) / 2d);
            top = DCSInterfaceFunctionTester.LABEL_HEIGHT + ROW_HEIGHT * Row;
            width = DCSInterfaceFunctionTester.CONTROL_WIDTH;
            height = DCSInterfaceFunctionTester.CONTROL_HEIGHT;
            Column += columns;

            if (Column * DCSInterfaceFunctionTester.CONTROL_WIDTH > Container.Width)
            {
                // next row, try again
                Column = columns;
                Row++;
                left = DCSInterfaceFunctionTester.CONTROL_WIDTH * (columns - 1d) / 2d;
                top += ROW_HEIGHT;
            }

            if (top + ROW_HEIGHT > Container.Height)
            {
                // out of space on panel
                AddPanel();

                left = DCSInterfaceFunctionTester.CONTROL_WIDTH * (columns - 1d) / 2d;
                top = DCSInterfaceFunctionTester.LABEL_HEIGHT;
                Row = 0;
                Column = columns;
            }

            // create label
            Controls.TextDecoration label = new Controls.TextDecoration
            {
                Name = $"Label {dcsFunction.DeviceName} {dcsFunction.Name}",
                Text = dcsFunction.Name,
                Left = left,
                Top = top + height,
                Width = width,
                Height = DCSInterfaceFunctionTester.LABEL_HEIGHT
            };
            label.Format.FontSize = DCSInterfaceFunctionTester.LABEL_FONT_SIZE;
            Container.Children.Add(label);
        }

        private void AddPanel()
        {
            string panelName = Containers.Any() ? $"{DeviceName} {Containers.Count + 1}" : DeviceName;

            Containers.Add(new Controls.HeliosPanel
            {
                Name = panelName,
                Left = 0,
                Width = TargetMonitor.Width,
                Top = TOP_SPACE,
                Height = TargetMonitor.Height - TOP_SPACE,
                IsDefaultHidden = true
            });

            Controls.TextDecoration panelTitle = new Controls.TextDecoration
            {
                Left = 20,
                Top = 0,
                Width = Container.Width,
                Height = DCSInterfaceFunctionTester.LABEL_HEIGHT,
                Text = DeviceName
            };
            panelTitle.Format.HorizontalAlignment = TextHorizontalAlignment.Left;
            panelTitle.Format.VerticalAlignment = TextVerticalAlignment.Center;
            panelTitle.Format.FontSize = 20;
            Container.Children.Add(panelTitle);

            TargetMonitor.Children.Add(Container);

            SelectButtons.Add(new Controls.PushButton
            {
                Name = $"select {panelName}",
                Left = 0,
                Width = SELECT_BUTTON_WIDTH,
                Top = 0,
                Height = SELECT_BUTTON_HEIGHT,
                Text = panelName
            });
            SelectButton.TextFormat.FontSize = Math.Ceiling(SELECT_BUTTON_HEIGHT / 3);
        }

        public static void ConnectButtons(List<Panel> panels)
        {
            // step through pairs of containers and their associated buttons
            foreach (Panel panel in panels)
            {
                using (List<Controls.PushButton>.Enumerator buttonEnumerator = panel.SelectButtons.GetEnumerator())
                {
                    foreach (Controls.HeliosPanel panelContainer in panel.Containers)
                    {
                        // move in lock step with containers enumeration
                        buttonEnumerator.MoveNext();

                        // hook up hide actions: every other button hides this panel
                        foreach (Controls.PushButton pushButton in panels
                            .SelectMany(p => p.SelectButtons, (p, b) => b)
                            .Where(b => b != buttonEnumerator.Current))
                        {
                            AddSetHiddenBinding(pushButton, "pushed", panelContainer, "true");
                        }

                        // this button shows this panel
                        AddSetHiddenBinding(buttonEnumerator.Current, "pushed", panelContainer, "false");
                    }
                }
            }
        }

        private static HeliosBinding AddSetHiddenBinding(HeliosVisual source, string triggerName, HeliosVisual target,
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

        #region Properties

        public Controls.HeliosPanel Container => Containers.Last();
        public Controls.PushButton SelectButton => SelectButtons.Last();
        public Monitor TargetMonitor { get; }
        public string DeviceName { get; }

        internal List<Controls.HeliosPanel> Containers { get; } = new List<Controls.HeliosPanel>();
        internal List<Controls.PushButton> SelectButtons { get; } = new List<Controls.PushButton>();

        private int Row { get; set; }
        private int Column { get; set; }

        #endregion
    }
}