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

using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.Util
{
    internal class OnceLogger
    {
        private Logger logger;
        private static HashSet<string> IdsLogged = new HashSet<string>();

        public OnceLogger(Logger logger)
        {
            this.logger = logger;
        }

        internal void InfoOnceUnlessDebugging(string loggingId, string message, params object[] args)
        {
            if (logger.IsDebugEnabled)
            {
                // log always in debug
                logger.Info(message, args);
                return;
            }

            if (IdsLogged.Add(loggingId))
            {
                // log once if logging at info
                List<object> modifiedArgs = new List<object> { "This event is logged only once" };
                modifiedArgs.AddRange(args);
                logger.Info($"{{Once}} {message}", modifiedArgs.ToArray());
            }
        }
    }
}