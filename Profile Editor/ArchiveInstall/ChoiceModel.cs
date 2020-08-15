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

using System.Windows;
using System.Windows.Input;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.ProfileEditor.ArchiveInstall
{
    public class ChoiceModel: HeliosStaticViewModel<Choice>
    {
        /// <summary>
        /// backing field for property OptionSelectionCommand, contains
        /// command handler for selection from options
        /// </summary>
        private ICommand _optionSelectionCommand;

        /// <summary>
        /// backing field for property OkCommand, contains
        /// handlers for Ok button
        /// </summary>
        private ICommand _okCommand;

        public ChoiceModel(Choice data) : base(data)
        {
            // no code
        }

        public string Title => "Archive Installation Choice Required";

        /// <summary>
        /// command handler for selection from options
        /// </summary>
        public ICommand OptionSelectionCommand
        {
            get
            {
                _optionSelectionCommand = _optionSelectionCommand ?? new RelayCommand(parameter =>
                {
                    Selected = (Option) parameter;
                });
                return _optionSelectionCommand;
            }
        }

        /// <summary>
        /// handlers for Ok button
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                _okCommand = _okCommand ?? new RelayCommand(
                    // Executed
                    parameter =>
                    {
                        // close dialog
                        if (parameter is IInputElement inputElement)
                        {
                            // fire a close command up the tree from the source of our command so it finds the right window
                            SystemCommands.CloseWindowCommand.Execute(null, inputElement);
                        }
                    },
                    // CanExecute
                    parameter => Selected != null
                );
               return _okCommand;
            }
        }

        public Option Selected { get; private set; }
    }
}
