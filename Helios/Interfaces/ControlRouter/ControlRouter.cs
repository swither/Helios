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

using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Controls.Capabilities;
using GadrocsWorkshop.Helios.Interfaces.Common;

namespace GadrocsWorkshop.Helios.Interfaces.ControlRouter
{
    [HeliosInterface("Helios.Base.ControlRouter", "Control Router", typeof(ControlRouterEditor),
        typeof(UniqueHeliosInterfaceFactory))]
    public class ControlRouter : HeliosInterfaceWithXml<ControlRouterModel>
    {
        private INamedControl _mostRecentlySelected;

        private readonly HeliosValue _mostRecentName;

        private readonly HeliosValue _unclaimedName;

        // deserialization constructor
        public ControlRouter() : this("Control Router")
        {
            // no code
        }

        public ControlRouter(string name) : base(name)
        {
            _mostRecentName = new HeliosValue(this, new BindingValue(""), "Most Recent Control", null,
                "most recently selected control", "name of the control", BindingValueUnits.Text);
            Values.Add(_mostRecentName);
            Triggers.Add(_mostRecentName);

            _unclaimedName = new HeliosValue(this, new BindingValue(""), "Unclaimed Control", null,
                "control that will be claimed by next active port", "name of the control", BindingValueUnits.Text);
            Values.Add(_unclaimedName);
            Triggers.Add(_unclaimedName);
        }

        internal bool TryClaimControl(out INamedControl control)
        {
            control = _mostRecentlySelected;
            if (_mostRecentlySelected == null)
            {
                return false;
            }

            // claimed
            _mostRecentlySelected = null;
            _unclaimedName.SetValue(new BindingValue(""), false);
            return true;
        }

        #region Event Handlers

        private void Profile_RoutableControlSelected(object sender, Controls.ControlEventArgs e)
        {
            _mostRecentlySelected = e.Control;
            string controlName = e.Control?.Name ?? "";
            _mostRecentName.SetValue(new BindingValue(controlName), false);
            _unclaimedName.SetValue(new BindingValue(controlName), false);
        }

        #endregion

        #region Overrides

        protected override void AttachToProfileOnMainThread()
        {
            base.AttachToProfileOnMainThread();
            if (Model.Ports.Count == 0)
            {
                // create some ports
                Model.CreateDefaultPorts(4);
            }

            Model.Ports.ForEach(model =>
            {
                model.Initialize(this);

                // we slave these collections so we can clear them later if we remove the port
                Actions.AddSlave(model.Actions);
                Triggers.AddSlave(model.Triggers);
                Values.AddSlave(model.Values);
            });

            // sign up for any compatible controls being touched
            Profile.RoutableControlSelected += Profile_RoutableControlSelected;
        }

        protected override void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            oldProfile.RoutableControlSelected -= Profile_RoutableControlSelected;
            base.DetachFromProfileOnMainThread(oldProfile);
        }

        public override void Reset()
        {
            base.Reset();
            _mostRecentlySelected = null;
            Model.Ports.ForEach(port => port.Reset());
        }

        #endregion
    }
}