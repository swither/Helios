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

using System.Windows;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// A HeliosVisual (control) that has a model instance that is serialized to the Profile XML
    ///
    /// The model class should encapsulate all the configuration of this control.  Undo support
    /// is provided for all properties in the instance "Model"
    /// </summary>
    /// <typeparam name="T">a default-constructable descendant of HeliosXmlModel</typeparam>
    public abstract class HeliosVisualWithXml<T> : HeliosVisual where T: HeliosXmlModel, new()
    {
        private T _model = new T();

        protected HeliosVisualWithXml(string name, Size nativeSize) : base(name, nativeSize)
        {
            // register to forward any configuration changes against default config
            _model.PropertyChanged += Model_PropertyChanged;
        }

        public T Model
        {
            get => _model;
            protected set
            {
                T oldModel = _model;
                _model = value;
                OnModelReplacement(oldModel, value);
            }
        }

        protected virtual void OnModelReplacement(T oldModel, T newModel)
        {
            if (oldModel != null)
            {
                oldModel.PropertyChanged -= Model_PropertyChanged;
            }

            if (newModel != null)
            {
                // register to forward any configuration value changes after load
                newModel.PropertyChanged += Model_PropertyChanged;
            }
        }

        protected virtual void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // forward changes to configuration properties as child property events so we get Undo support
            OnPropertyChanged(nameof(Model), (PropertyNotificationEventArgs)e);
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            Model = HeliosXmlModel.ReadXml<T>(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            HeliosXmlModel.WriteXml(writer, Model);
        }
    }
}