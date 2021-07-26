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

using System.IO;

namespace GadrocsWorkshop.Helios.Util
{
    internal static class ModuleUtility
    {
        // adapted from https://stackoverflow.com/a/29643803/955263 and ported to 64 bit
        internal static bool IsProbablyAssembly(string moduleFileName)
        {
            Stream fs = new FileStream(moduleFileName, FileMode.Open, FileAccess.Read);
            try
            {
                BinaryReader reader = new BinaryReader(fs);

                // PE Header starts @ 0x3C (60). Its a 4 byte header.
                fs.Position = 0x3C;

                uint peHeader = reader.ReadUInt32();
                fs.Position = peHeader;
                
                // ReSharper disable UnusedVariable
                uint peHeaderSignature = reader.ReadUInt32();
                
                ushort machine = reader.ReadUInt16();
                ushort sections = reader.ReadUInt16();
                uint timestamp = reader.ReadUInt32();
                uint pSymbolTable = reader.ReadUInt32();
                uint noOfSymbol = reader.ReadUInt32();
                ushort sizeOfOptionalHeader = reader.ReadUInt16();
                ushort characteristics = reader.ReadUInt16();

                // skip ahead to data directory 15, the position of which depends on the
                // module format
                ushort optionalMagic = reader.ReadUInt16();
                ushort targetOffset;
                switch (optionalMagic)
                {
                    case 0x10b:
                        // PE32
                        targetOffset = 96 + 14 * 8;
                        break;
                    case 0x20b:
                        // PE32+
                        targetOffset = 112 + 14 * 8;
                        break;
                    default:
                        // unsupported
                        return false;
                }

                if (targetOffset + 4 > sizeOfOptionalHeader)
                {
                    // overrun
                    return false;
                }
                
                // skip ahead, except for the 2 bytes of magic we read
                fs.Position += (targetOffset - 2);

                // ReSharper restore UnusedVariable

                return reader.ReadUInt32() != 0;
            }
            finally
            {
                fs.Close();
            }
        }
    }
}