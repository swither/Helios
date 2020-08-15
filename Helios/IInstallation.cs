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

using System.Collections.Generic;

namespace GadrocsWorkshop.Helios
{
    public enum InstallationPromptResult
    {
        Ok,
        Cancel
    }

    public enum InstallationResult
    {
        Success,
        Fatal,
        Canceled
    }

    /// <summary>
    /// callbacks from configuration attempts that may be implemented by a UI or otherwise
    /// </summary>
    public interface IInstallationCallbacks
    {
        /// <summary>
        /// Presents a warning, allowing for review of the details and selection of Ok or Cancel for the current configuration attempt
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="details"></param>
        /// <returns>the user's selection</returns>
        InstallationPromptResult DangerPrompt(string title, string message, IList<StatusReportItem> details);

        /// <summary>
        /// Presents a failure result including details, representing a failed configuration attempt with no option to retry
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="details"></param>
        void Failure(string title, string message, IList<StatusReportItem> details);

        /// <summary>
        /// Presents a success result including details, representing a successful configuration attempt.  This message
        /// may not be shown to the user depending on configured options.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="details"></param>
        /// <returns>true if the message was shown to the user</returns>
        bool Success(string title, string message, IList<StatusReportItem> details);

        /// <summary>
        /// Presents a non-optional message during a configuration attempt that has not yet concluded, i.e. there will be a
        /// Failure, Success, or DangerPrompt cancellation later.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        void ImportantMessage(string title, string message);
    }

    /// <summary>
    /// support for making a configuration attempt by a Helios component
    /// </summary>
    public interface IInstallation
    {
        InstallationResult Install(IInstallationCallbacks callbacks);
    }
}