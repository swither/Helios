﻿// Copyright 2021 Ammo Goettsch
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
using System.Collections.Generic;
using System.IO;
using GadrocsWorkshop.Helios.Json;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Soft
{
    /// <summary>
    /// custom factory creates DCS interface instances from interface files
    /// </summary>
    public class SoftInterfaceFactory : UniqueHeliosInterfaceFactory
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal static string FindInterfaceFileByName(string name)
        {
            // NOTE: we only instantiate one of these per profile, and we always want latest information,
            // so there is no reason to cache or index anything and we just linear search all the headers
            // XXX measure how long this takes and see if maybe we have to something like index caching based on file checksums
            // XXX merge installed and user interfaces
            string userInterfaceSpecsFolder = InterfaceHeader.UserInterfaceSpecsFolder;
            Logger.Info("Searching for interface definition for {Name} in {Path}", name, Anonymizer.Anonymize(userInterfaceSpecsFolder));
            foreach (string specFilePath in Directory.EnumerateFiles(userInterfaceSpecsFolder, "*.hif.json",
                SearchOption.AllDirectories))
            {
                InterfaceHeader header = InterfaceHeader.LoadHeader(specFilePath);
                if (header.Type == InterfaceHeader.InterfaceType.DCS &&
                    header.Name == name)
                {
                    Logger.Info("Found interface definition at {Path}", Anonymizer.Anonymize(specFilePath));
                    return specFilePath;
                }
            }

            return null;
        }

        // enumerate all instances we could create without loading them fully
        private static IEnumerable<HeliosInterface> LoadSoftInterfacesFromFolder(string interfaceSpecsFolder)
        {
            if (!Directory.Exists(interfaceSpecsFolder))
            {
                Logger.Debug("Soft interface folder {Path} not found", interfaceSpecsFolder);
                yield break;
            }

            Logger.Info("Searching for interface definitions in {Path}", Anonymizer.Anonymize(interfaceSpecsFolder));
            foreach (string specFilePath in Directory.EnumerateFiles(interfaceSpecsFolder, "*.hif.json",
                SearchOption.AllDirectories))
            {
                InterfaceHeader header = InterfaceHeader.LoadHeader(specFilePath);
                if (header.Type == InterfaceHeader.InterfaceType.DCS)
                {
                    Logger.Info("Found DCS interface {Name} at {Path}", header.Name, Anonymizer.Anonymize(specFilePath));

                    // create candidate instance, without loading it fully
                    yield return new SoftInterface(header.Name, header.Module, specFilePath);
                }
            }
        }

        #region Overrides

        public override List<HeliosInterface> GetInterfaceInstances(HeliosInterfaceDescriptor descriptor,
            HeliosProfile profile)
        {
            List<HeliosInterface> interfaces = new List<HeliosInterface>();

            if (!(ConfigManager.Application.AllowSoftInterfaces && GlobalOptions.HasAllowSoftInterfaces))
            {
                Logger.Debug("Soft interfaces disabled on this installation");
                return interfaces;
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor),
                    @"cannot load interfaces from null descriptor; broken implementation");
            }

            if (!IsUnique(descriptor, profile))
            {
                Logger.Debug("SoftInterface cannot coexist with interface already in profile");
                return interfaces;
            }

            // XXX load installed interfaces

            // XXX merge these user interfaces instead of just loading them
            interfaces.AddRange(LoadSoftInterfacesFromFolder(InterfaceHeader.UserInterfaceSpecsFolder));

            return interfaces;
        }

        public override List<HeliosInterface> GetAutoAddInterfaces(HeliosInterfaceDescriptor descriptor,
            HeliosProfile profile) => new List<HeliosInterface>();

        #endregion
    }
}