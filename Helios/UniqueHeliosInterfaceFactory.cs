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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface factory interface which only allows one instance of the interface type per profile.
    /// </summary>
    public class UniqueHeliosInterfaceFactory : HeliosInterfaceFactory
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override List<HeliosInterface> GetInterfaceInstances(HeliosInterfaceDescriptor descriptor, HeliosProfile profile)
        {
            List<HeliosInterface> interfaces = new List<HeliosInterface>();

            if (descriptor == null)
            {
                Logger.Warn("Descriptor is null passed into UniqueHeliosInterfaceFactory.GetInterfaceInstances.");
            }
            else
            {
                if (IsUnique(descriptor, profile))
                {
                    HeliosInterface newInterface = (HeliosInterface)Activator.CreateInstance(descriptor.InterfaceType);
                    if (newInterface == null)
                    {
                        Logger.Warn("New interface is null.");
                    }
                    interfaces.Add(newInterface);
                }
                else
                {
                    Logger.Debug("Unique interface already exists in profile " + descriptor.Name + ". Type: " + descriptor.InterfaceType.BaseType.Name);
                }
            }

            return interfaces;
        }

        public override List<HeliosInterface> GetAutoAddInterfaces(HeliosInterfaceDescriptor descriptor, HeliosProfile profile)
        {
            List<HeliosInterface> interfaces = new List<HeliosInterface>();

            if (descriptor != null && descriptor.AutoAdd && IsUnique(descriptor, profile))
            {
                interfaces.Add((HeliosInterface)Activator.CreateInstance(descriptor.InterfaceType));
            }

            return interfaces;
        }

        // XXX this hack is going away in helios17 and interface2 branches
        private static readonly HashSet<string> _udpInterfaceTypes = new HashSet<string>
        {
            "BaseUDPInterface",
            "DCSInterface"
        };

        private bool IsUnique(HeliosInterfaceDescriptor descriptor, HeliosProfile profile)
        {
            foreach (HeliosInterface heliosInterface in profile.Interfaces)
            {
                HeliosInterfaceDescriptor interfaceDescriptor = ConfigManager.ModuleManager.InterfaceDescriptors[heliosInterface.GetType()];
                if (interfaceDescriptor.TypeIdentifier.Equals(descriptor.TypeIdentifier))
                {
                    // If any existing interfaces in the profile have the same type identifier do not add them.
                    return false;
                }

                // XXX this hack is going away in helios17 and interface2 branches
                if (_udpInterfaceTypes.Contains(descriptor.InterfaceType.BaseType.Name) &&
                    _udpInterfaceTypes.Contains(heliosInterface.GetType().BaseType.Name))
                {
                    // don't add descendants of BaseUDPInterface
                    return false;
                }
            }

            return true;
        }
    }
}
