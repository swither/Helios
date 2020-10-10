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

using System.Collections.Generic;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// callbacks from configuration attempts that may be implemented by a UI or otherwise
    ///
    /// version 2 of interface that supports structured information
    /// </summary>
    public interface IInstallationCallbacks2: IInstallationCallbacks
    {
        /// <summary>
        /// Presents a warning, allowing for review of the details and selection of Ok or Cancel for the current configuration attempt
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="info"></param>
        /// <param name="status"></param>
        /// <returns>the user's selection</returns>
        InstallationPromptResult DangerPrompt(string title, string message, IList<StructuredInfo> info, IList<StatusReportItem> status);

        /// <summary>
        /// Presents a failure result including details, representing a failed configuration attempt with no option to retry
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="info"></param>
        /// <param name="status"></param>
        void Failure(string title, string message, IList<StructuredInfo> info, IList<StatusReportItem> status);

        /// <summary>
        /// Presents a success result including details, representing a successful configuration attempt.  This message
        /// may not be shown to the user depending on configured options.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="info"></param>
        /// <param name="status"></param>
        /// <returns>true if the message was shown to the user</returns>
        bool Success(string title, string message, IList<StructuredInfo> info, IList<StatusReportItem> status);

        /// <summary>
        /// Presents a non-optional message during a configuration attempt that has not yet concluded, i.e. there will be a
        /// Failure, Success, or DangerPrompt cancellation later.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="info"></param>
        void ImportantMessage(string title, string message, IList<StructuredInfo> info);
    }

    /// <summary>
    /// support for making a configuration attempt by a Helios component (version 2)
    /// </summary>
    public interface IInstallation2
    {
        /// <summary>
        /// install whatever it is that this component installs
        /// </summary>
        /// <param name="callbacks"></param>
        /// <returns></returns>
        InstallationResult Install(IInstallationCallbacks2 callbacks);
    }
}