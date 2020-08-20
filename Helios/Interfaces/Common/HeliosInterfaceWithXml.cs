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

using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Interfaces.Common
{
    /// <summary>
    /// A Helios Interface implementation that has a model instance that is serialized to the XML
    /// under the interface node in the Profile
    ///
    /// The model class should encapsulate all the configuration of this interface.  Undo support
    /// is provided for all properties in the instance "Model"
    /// </summary>
    /// <typeparam name="T">a default-constructable descendant of HeliosXmlModel</typeparam>
    public class HeliosInterfaceWithXml<T> : HeliosInterface where T : HeliosXmlModel, new()
    {
        private T _model = new T();

        public HeliosInterfaceWithXml(string name) : base(name)
        {
            // register to forward any configuration changes against default config
            _model.PropertyChanged += Model_PropertyChanged;
        }

        public T Model
        {
            get => _model;
            protected set
            {
                if (_model != null)
                {
                    _model.PropertyChanged -= Model_PropertyChanged;
                }
                _model = value;
                if (_model != null)
                {
                    // register to forward any configuration value changes after load
                    _model.PropertyChanged += Model_PropertyChanged;
                }

            }
        }

        protected virtual void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // forward changes to configuration properties as child property events so we get Undo support
            OnPropertyChanged(nameof(Model), (PropertyNotificationEventArgs)e);
        }

        public override void ReadXml(XmlReader reader)
        {
            Model = HeliosXmlModel.ReadXml<T>(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {
            HeliosXmlModel.WriteXml(writer, Model);
        }
    }
}