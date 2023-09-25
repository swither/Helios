//  Copyright 2014 Craig Courtney
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;


namespace GadrocsWorkshop.Helios.Controls.Special
{
    public class ScreenReplicatorRenderer : HeliosVisualRenderer
    {
        private Rect _displayRect = new Rect(0, 0, 0, 0);
        private Brush _inactiveBrush;
        private Pen _inactivePen;
        private TextFormat _inactiveTextFormat;
        private IntPtr _hDC;
        private IntPtr _hMemDC;
        private IntPtr _hBitmap;
        private ImageBrush _screenBrush;

        public ScreenReplicatorRenderer()
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!(Visual is ScreenReplicator replicator))
            {
                return;
            }

            if (replicator.IsRunning)
            {
                if (replicator.IsReplicating)
                {
                    _screenBrush = CreateImageBrush(replicator);
                }
                else if (replicator.BlankOnStop)
                {
                    _screenBrush = null;
                }

                if (_screenBrush != null)
                {
                    drawingContext.DrawRectangle(_screenBrush, null, _displayRect);
                }
            }
            else
            {
                drawingContext.DrawRectangle((ConfigManager.Application.ShowDesignTimeControls) ? new SolidColorBrush(Color.FromArgb(0x40, 0x64, 0x95, 0xed)) : null, _inactivePen, _displayRect);
                _inactiveTextFormat.RenderText(drawingContext, _inactiveBrush, replicator.Name, _displayRect);
            }
        }

        protected override void OnRefresh()
        {
            _inactiveBrush = Brushes.BlueViolet;
            _inactivePen = new Pen(_inactiveBrush, 1d) { DashStyle = DashStyles.Dash };
            _inactiveTextFormat = new TextFormat
            {
                HorizontalAlignment = TextHorizontalAlignment.Center,
                VerticalAlignment = TextVerticalAlignment.Center
            };

            if (Visual is ScreenReplicator replicator)
            {
                _displayRect.Width = replicator.Width;
                _displayRect.Height = replicator.Height;
            }
        }

        virtual protected ImageBrush CreateImageBrush(ScreenReplicator replicator)
        {
            ImageBrush brush = null;
            _hDC = NativeMethods.GetDC(NativeMethods.GetDesktopWindow());
            _hMemDC = NativeMethods.CreateCompatibleDC(_hDC);
            _hBitmap = NativeMethods.CreateCompatibleBitmap(_hDC, replicator.CaptureRectangle.Width, replicator.CaptureRectangle.Height);
            if (_hBitmap != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)NativeMethods.SelectObject(_hMemDC, _hBitmap);

                NativeMethods.BitBlt(_hMemDC, 0, 0, replicator.CaptureRectangle.Width, replicator.CaptureRectangle.Height, _hDC,
                                           replicator.CaptureRectangle.X, replicator.CaptureRectangle.Y, NativeMethods.SRCCOPY);

                NativeMethods.SelectObject(_hMemDC, hOld);
                BitmapSource bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(_hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(replicator.CaptureRectangle.Width, replicator.CaptureRectangle.Height));

                brush = new ImageBrush(bmp);
                NativeMethods.DeleteObject(_hBitmap);
            }
            NativeMethods.DeleteDC(_hMemDC);
            NativeMethods.ReleaseDC(NativeMethods.GetDesktopWindow(), _hDC);
            return brush;
        }
    }
}
