﻿//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class BacklitPushButtonRenderer : HeliosVisualRenderer
    {
        private PathGeometry _glyphPath;

        private ImageSource _image;
        private Rect _imageRect;
        private Brush _textBrush;

        private Brush _glyphBrush;
        private Pen _glyphPen;

        protected override void OnRender(DrawingContext drawingContext)
        {
            BacklitPushButton pushButton = Visual as BacklitPushButton;

            double colorWidth, colorHeight, colorOffsetX, colorOffsetY;
            colorWidth = pushButton.Width * pushButton.BacklitColorScale;
            colorHeight = pushButton.Height * pushButton.BacklitColorScale;
            colorOffsetX = (pushButton.Width - colorWidth) / 2;
            colorOffsetY = (pushButton.Height - colorHeight) / 2;
            Rect colorRect = new Rect(0, 0, colorWidth, colorHeight);
            colorRect.Offset(new Vector(colorOffsetX, colorOffsetY));

            if (pushButton.Pushed && _image != null)
            {
                drawingContext.DrawRectangle(new SolidColorBrush(pushButton.PushBackingColor), null, colorRect);
                drawingContext.DrawImage(_image, _imageRect);
            }
            else if (_image != null)
            {
                drawingContext.DrawRectangle(new SolidColorBrush(pushButton.NormalBackingColor), null, colorRect);
                drawingContext.DrawImage(_image, _imageRect);
            }

            if (pushButton.Pushed)
            {
                drawingContext.PushTransform(new TranslateTransform(pushButton.TextPushOffset.X, pushButton.TextPushOffset.Y));
            }

            if (pushButton.Glyph != PushButtonGlyph.None)
            {
                drawingContext.DrawGeometry(_glyphBrush, _glyphPen, _glyphPath);
            }
            pushButton.TextFormat.RenderText(drawingContext, _textBrush, pushButton.Text, _imageRect);

            if (pushButton.Pushed)
            {
                drawingContext.Pop();
            }
        }

        private void RenderGlyph()
        {
        }

        protected override void OnRefresh()
        {
            BacklitPushButton pushButton = Visual as BacklitPushButton;
            if (pushButton != null)
            {
                _imageRect.Width = pushButton.Width;
                _imageRect.Height = pushButton.Height;
                _image = ConfigManager.ImageManager.LoadImage(pushButton.Image);
                _textBrush = new SolidColorBrush(pushButton.TextColor);

                _glyphBrush = new SolidColorBrush(pushButton.GlyphColor);
                _glyphPen = new Pen(_glyphBrush, pushButton.GlyphThickness);

                Point center = new Point(pushButton.Width / 2d, pushButton.Height / 2d);

                _glyphPath = new PathGeometry();
                switch (pushButton.Glyph)
                {
                    case PushButtonGlyph.Circle:
                        double radius = (Math.Min(pushButton.Width, pushButton.Height) / 2d) * pushButton.GlyphScale;
                        EllipseGeometry ellipse = new EllipseGeometry(center, radius, radius);
                        _glyphBrush = null;
                        _glyphPath.AddGeometry(ellipse);
                        break;

                    case PushButtonGlyph.RightArrow:
                        _glyphPath.Figures.Add(GetArrowFigure(true, pushButton));
                        break;

                    case PushButtonGlyph.LeftArrow:
                        _glyphPath.Figures.Add(GetArrowFigure(false, pushButton));
                        break;

                    case PushButtonGlyph.UpCaret:
                        double offsetX = center.X * pushButton.GlyphScale;
                        double offsetY = offsetX / 2d;
                        PathFigure figure = new PathFigure();
                        figure.IsClosed = false;
                        figure.IsFilled = false;
                        figure.StartPoint = new Point(center.X - offsetX, center.Y + offsetY);
                        figure.Segments.Add(new LineSegment(new Point(center.X, center.Y - offsetY), true));
                        figure.Segments.Add(new LineSegment(new Point(center.X + offsetX, center.Y + offsetY), true));
                        _glyphPath.Figures.Add(figure);
                        break;
                }
            }
            else
            {
                _image = null;
            }
        }

        private PathFigure GetArrowFigure(bool Right, BacklitPushButton pushButton)
        {
            double y = pushButton.Height / 2d;
            double arrowLength = pushButton.Width * pushButton.GlyphScale;
            double padding = (pushButton.Width - arrowLength) / 2d;
            double arrowLineLength = arrowLength * .6d;
            double headHeightOffset = pushButton.GlyphThickness * 2d;

            Point lineStart, lineEnd, head, head1, head2;

            if (Right)
            {
                lineStart = new Point(padding, y);
                lineEnd = new Point(lineStart.X + arrowLineLength, y);
                head = new Point(pushButton.Width - padding, y);
                head1 = new Point(lineEnd.X, y - headHeightOffset);
                head2 = new Point(lineEnd.X, y + headHeightOffset);
            }
            else
            {
                lineStart = new Point(pushButton.Width - padding, y);
                lineEnd = new Point(lineStart.X - arrowLineLength, y);
                head = new Point(padding, y);
                head2 = new Point(lineEnd.X, y - headHeightOffset);
                head1 = new Point(lineEnd.X, y + headHeightOffset);
            }

            PathFigure arrow = new PathFigure();
            arrow.StartPoint = lineStart;
            arrow.Segments.Add(new LineSegment(lineEnd, true));
            arrow.Segments.Add(new LineSegment(head1, false));
            arrow.Segments.Add(new LineSegment(head, false));
            arrow.Segments.Add(new LineSegment(head2, false));
            arrow.Segments.Add(new LineSegment(lineEnd, false));

            return arrow;
        }
    }
}
