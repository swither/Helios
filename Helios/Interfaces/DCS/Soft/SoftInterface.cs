// Copyright 2021 Ammo Goettsch
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
using System.IO;
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Soft
{
    public class SoftInterface : DCSInterface
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public SoftInterface(string name, string exportDeviceName, string specFilePath) : base(name,
            exportDeviceName, Path.ChangeExtension(specFilePath, "lua"))
        {
            SpecFilePath = specFilePath;

            if (!File.Exists(ExportFunctionsPath))
            {
                // not all interfaces have custom Lua code, that is ok
                Logger.Info("no custom Lua export functions found at {Path}; there will be no custom code in the associated interface", Anonymizer.Anonymize(ExportFunctionsPath));
                
                // tell export generator to not even try to load this
                ExportFunctionsPath = null;
            }
        }

        public string SpecFilePath { get; }

        #region Overrides of DCSInterface

        protected override void AttachToProfileOnMainThread()
        {
            // we are being instantiated for use, not just for the interface picker, so now we really load
            Json.InterfaceFile<UDPInterface.NetworkFunction> functions = Json.InterfaceFile<UDPInterface.NetworkFunction>.LoadFunctions(this, SpecFilePath);
            InstallFunctions(functions);
            
            // now we are completely defined; start up
            base.AttachToProfileOnMainThread();
        }

        #endregion
    }

    public class SoftInterfaceDescriptor : HeliosInterfaceDescriptor
    {
        public SoftInterfaceDescriptor(string name, string typeIdentifier, string moduleName, string specFilePath)
            : base(typeof(SoftInterface), name, typeIdentifier, null, typeof(DCSInterfaceEditor),
                typeof(UniqueHeliosInterfaceFactory), "Helios.DCSInterface", false)
        {
            SpecFilePath = specFilePath;
            ModuleName = moduleName;
        }

        #region Overrides

        public override HeliosInterface CreateInstance() =>
            new SoftInterface(Name, ModuleName, SpecFilePath);

        public override HeliosInterface CreateInstance(HeliosInterface parent) =>
            throw new Exception($"{typeof(SoftInterface)} does not support parent interfaces");

        public string SpecFilePath { get; }

        #endregion

        #region Properties

        public string ModuleName { get; }

        #endregion
    }
}