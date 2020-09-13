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
using System.Linq;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Tools;
using GadrocsWorkshop.Helios.Tools.Capabilities;

namespace GadrocsWorkshop.Helios.ProfileEditorTools
{
    /// <summary>
    /// tool to set all output bindings on all interfaces to no cascade
    /// </summary>
    [HeliosTool]
    public class CascadeRemover : ProfileTool, IMenuSectionFactory
    {
        public MenuSectionModel CreateMenuSection()
        {
            return new MenuSectionModel("Cascade Remover", new List<MenuItemModel>
            {
                new MenuItemModel("Set all interface bindings to not cascade", new Windows.RelayCommand(
                    parameter => { Execute(); },
                    parameter => CanStart))
            });
        }

        /// <summary>
        /// since interface bindings are broken in Helios Undo system (property changes are not forwarded to interface for Undo)
        /// we create our own undo context that marks the profile as dirty and also allows this operation to be reversed
        /// </summary>
        private class CascadeRemoval : IUndoItem
        {
            private readonly List<HeliosBinding> _items;

            public bool DoesSomething => _items.Any();

            public CascadeRemoval(List<HeliosBinding> items)
            {
                _items = items;
            }

            public void Undo()
            {
                _items.ForEach(i => i.BypassCascadingTriggers = false);
            }

            public void Do()
            {
                _items.ForEach(i => i.BypassCascadingTriggers = true);
            }
        }

        private void Execute()
        {
            if (Profile == null)
            {
                return;
            }

            CascadeRemoval operation = new CascadeRemoval(FindInterfaceOutputCascades().ToList());
            if (!operation.DoesSomething)
            {
                return;
            }
            operation.Do();
            ConfigManager.UndoManager.AddUndoItem(operation);
        }

        private IEnumerable<HeliosBinding> FindInterfaceOutputCascades()
        {
            return Profile.Interfaces.SelectMany(profileInterface => profileInterface.OutputBindings
                .Where(h => !h.BypassCascadingTriggers));
        }
    }
}