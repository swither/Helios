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

using System;
using System.IO;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    public class DCSOptions
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public class GraphicsTable
        {
            public GraphicsTable(long width, long height, string multiMonitorSetup, bool fullscreen)
            {
                Width = width;
                Height = height;
                MultiMonitorSetup = multiMonitorSetup;
                FullScreen = fullscreen;
            }

            public long Width { get; }
            public long Height { get; }

            public string MultiMonitorSetup { get; }
            public bool FullScreen { get; }
        }

        public GraphicsTable Graphics { get; private set; }

        private DCSOptions()
        {
            // no code
        }

        /// <summary>
        /// parses the options.lua file from DCS or returns false if it cannot
        /// </summary>
        /// <param name="location"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool TryReadOptions(InstallationLocation location, out DCSOptions options)
        {
            string optionsPath = location.OptionsPath;
            if (!File.Exists(optionsPath))
            {
                options = null;
                return false;
            }

            try
            {
                string optionsText = File.ReadAllText(optionsPath);
                NLua.Lua parser = new NLua.Lua();
                parser.DoString(optionsText, "options.lua");
                object graphics = parser.GetObjectFromPath("options.graphics");
                if (graphics is NLua.LuaTable graphicsTable && 
                    graphicsTable["width"] is long width &&
                    graphicsTable["height"] is long height &&
                    graphicsTable["multiMonitorSetup"] is string multiMonitorSetup &&
                    graphicsTable["fullScreen"] is bool fullscreen)
                {
                    options = new DCSOptions
                    {
                        Graphics = new GraphicsTable(width, height, multiMonitorSetup, fullscreen)
                    };
                    return true;
                }
            }
            catch (Exception ex)
            {
                // this is sort of ok, we might not have access, so we just don't check this part
                Logger.Info(ex, "failed to read DCS-owned options.lua file; Helios will not be able to check settings");
            }

            options = null;
            return false;
        }
    }
}