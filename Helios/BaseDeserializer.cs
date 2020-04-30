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

namespace GadrocsWorkshop.Helios
{
    using System.Windows;

    public class BaseDeserializer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #region Object Creation Methods

        protected object CreateNewObject(string type, string typeId)
        {
            switch (type)
            {
                case "Monitor":
                    return new Monitor();

                case "Visual":
                    HeliosVisual visual = ConfigManager.ModuleManager.CreateControl(typeId);
                    if (visual == null)
                    {
                        Logger.Error("Ignoring control not supported by this version of Helios: " + typeId);
                        return null;
                    }
                    visual.Dispatcher = Application.Current.Dispatcher;
                    return visual;

                case "Interface":
                    HeliosInterfaceDescriptor descriptor = ConfigManager.ModuleManager.InterfaceDescriptors[typeId];
                    if (descriptor == null)
                    {
                        Logger.Error("Ignoring interface not supported by this version of Helios: " + typeId);
                        return null;
                    }
                    HeliosInterface heliosInterface = descriptor != null ? descriptor.CreateInstance() : null;
                    if (heliosInterface != null)
                    {
                        heliosInterface.Dispatcher = Application.Current.Dispatcher;
                    }                    
                    return heliosInterface;

                case "Binding":
                    return new HeliosBinding();

            }
            return null;
        }

        #endregion
    }
}
