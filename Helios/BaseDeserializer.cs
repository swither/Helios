//  Copyright 2014 Craig Courtney
//  Copyright 2020 Ammo Goettsch
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
using System.Collections.Generic;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Unsupported;

namespace GadrocsWorkshop.Helios
{
    public class BaseDeserializer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected Dictionary<string, HeliosInterfaceDescriptor> DiscoveredAliases { get; set; }

        public HeliosInterface CreateNewInterface(string typeId, HeliosInterfaceCollection loaded, ComponentUnsupportedSeverity ifUnsupported = ComponentUnsupportedSeverity.Error)
        {
            HeliosInterfaceDescriptor descriptor = ConfigManager.ModuleManager.InterfaceDescriptors[typeId];
            
            if (descriptor == null)
            {
                switch (ifUnsupported)
                {
                    case ComponentUnsupportedSeverity.Error:
                        Logger.Error("Ignoring interface not supported by this version of Helios: {TypeId}; bindings to this interface will fail", typeId);
                        return null;
                    case ComponentUnsupportedSeverity.Warning:
                        Logger.Warn("Ignoring interface not supported by this version of Helios: {TypeId}; bindings to this interface will fail", typeId);
                        return null;
                    case ComponentUnsupportedSeverity.Ignore:
                        Logger.Info("Ignoring interface not supported by this version of Helios: {TypeId}; bindings to this interface will be silently ignored", typeId);
                        // create a dummy interface that preserves the XML and ignores but allows writing of all bindings
                        UnsupportedInterface dummy = new UnsupportedInterface();
                        dummy.RepresentedTypeIdentifier = typeId;

                        // record interface type alias, but don't install it yet because we may read more instances of this interface
                        // alias will allow resolution of the unsupported class to our dummy for use in places where we try to instantiate the editor
                        HeliosInterfaceDescriptor dummyDescriptor = ConfigManager.ModuleManager.InterfaceDescriptors[UnsupportedInterface.TYPE_IDENTIFIER];
                        if (DiscoveredAliases != null)
                        {
                            DiscoveredAliases[typeId] = dummyDescriptor;
                        }
                        return dummy;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ifUnsupported), ifUnsupported, null);
                }
            }

            HeliosInterface heliosInterface = null;
            if (descriptor.ParentTypeIdentifier != null)
            {
                foreach (HeliosInterface candidate in loaded)
                {
                    if (candidate.TypeIdentifier != descriptor.ParentTypeIdentifier)
                    {
                        continue;
                    }

                    // bind to first matching interface
                    heliosInterface = descriptor.CreateInstance(candidate);
                    break;
                }
                if (heliosInterface == null)
                {
                    ConfigManager.LogManager.LogError($"Child interface {typeId} could not locate its parent {descriptor.ParentTypeIdentifier}; interface not loaded");
                }
            } 
            else
            {
                heliosInterface = descriptor.CreateInstance();
            }
            return heliosInterface;
        }

        public object CreateNewObject(string type, string typeId, ComponentUnsupportedSeverity ifUnsupported = ComponentUnsupportedSeverity.Error)
        {
            switch (type)
            {
                case "Monitor":
                    return new Monitor();

                case "Visual":
                    HeliosVisual visual = ConfigManager.ModuleManager.CreateControl(typeId);
                    if (visual != null)
                    {
                        return visual;
                    }

                    Logger.Error("Ignoring control not supported by this version of Helios: " + typeId);
                    return null;

                case "Binding":
                    return new HeliosBinding();
            }
            return null;
        }
    }
}
