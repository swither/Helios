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

using System.Windows;
using System.Xml;
using GadrocsWorkshop.Helios.Controls.Capabilities;

namespace GadrocsWorkshop.Helios.Gauges
{
    public abstract class CompositeVisualWithBackgroundImage
        : CompositeVisual, IConfigurableBackgroundImage
    {
        /// <summary>
        /// backing field for property BackgroundImage, contains
        /// the Helios image path for the background/bezel image shown below all child controls
        /// </summary>
        private string _backgroundImage;

        /// <summary>
        /// to be initialized by descendant classes
        /// </summary>
        public abstract string DefaultBackgroundImage { get; }

        /// <summary>
        /// the Helios image path for the background/bezel image shown below all child controls
        /// </summary>
        public string BackgroundImage
        {
            get => _backgroundImage;
            set
            {
                if (_backgroundImage != null && _backgroundImage == value) return;
                string oldValue = _backgroundImage;
                _backgroundImage = value;
                OnPropertyChanged("BackgroundImage", oldValue, value, true);
                Refresh();
            }
        }

        protected CompositeVisualWithBackgroundImage(string name, Size nativeSize) : base(name, nativeSize)
        {
            // use default image unless changed during ReadXML
            // ReSharper disable once VirtualMemberCallInConstructor we don't have a deferred construction call from CreateControl
            _backgroundImage = DefaultBackgroundImage;
        }

        /// <summary>
        /// must be called by any overrides of this function
        /// </summary>
        /// <param name="writer"></param>
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            if (BackgroundImage != DefaultBackgroundImage)
            {
                writer.WriteElementString("BackgroundImage", BackgroundImage);
            }
        }

        /// <summary>
        /// must be called by any overrides of this function
        /// </summary>
        /// <param name="reader"></param>
        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            if (reader.Name == "BackgroundImage")
            {
                _backgroundImage = reader.ReadElementString("BackgroundImage");
            }
        }
    }
}