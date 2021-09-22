// Copyright 2020 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Windows;
using System.Windows.Input;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Controls.Capabilities;

namespace GadrocsWorkshop.Helios.Controls.Special
{
    /// <summary>
    /// this control is not available in release builds and is just used for testing or example code
    /// </summary>
#if DEBUG
    [HeliosControl("Helios.Base.Special.TestControl", NAME, "Development", typeof(RectangleDecorationRenderer))]
#endif
    public class TestControl : RectangleDecoration, IWindowsMouseInput, IWindowsPreviewInput
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private const string NAME = "Test Control";

        public TestControl() : base(NAME, new Size(200d, 100d))
        {
            // no code
        }

        #region Event Handlers

        public void GotMouseCapture(object sender, MouseEventArgs e)
        {
            Logger.Debug("GotMouseCapture");
            e.Handled = true;
        }

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
            Logger.Debug("LostMouseCapture");
            e.Handled = true;
        }

        public void MouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Logger.Debug("MouseDown");
            mouseButtonEventArgs.Handled = true;
        }

        public void MouseEnter(object sender, MouseEventArgs e)
        {
            Logger.Debug("MouseEnter");
            e.Handled = true;
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
            Logger.Debug("MouseLeave");
            e.Handled = true;
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            Logger.Debug("MouseMove");
            e.Handled = true;
        }

        public void MouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Logger.Debug("MouseUp");
            mouseButtonEventArgs.Handled = true;
        }

        public void PreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Logger.Debug("PreviewMouseDown");
        }

        public void PreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Logger.Debug("PreviewMouseUp");
        }

        public void PreviewTouchDown(object sender, TouchEventArgs touchEventArgs)
        {
            Logger.Debug("PreviewTouchDown");
        }

        public void PreviewTouchUp(object sender, TouchEventArgs touchEventArgs)
        {
            Logger.Debug("PreviewTouchUp");
        }

        #endregion

        #region Overrides

        public override void MouseDown(Point location)
        {
            throw new Exception("Helios MouseDown should not be called because we implement IWindowsMouseInput");
        }

        public override void MouseDrag(Point location)
        {
            throw new Exception("Helios MouseDrag should not be called because we implement IWindowsMouseInput");
        }

        public override void MouseUp(Point location)
        {
            throw new Exception("Helios MouseUp should not be called because we implement IWindowsMouseInput");
        }

        #endregion
    }
}