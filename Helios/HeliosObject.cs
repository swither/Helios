//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
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

using System.Linq;

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Windows.Threading;
    using System.Xml;

    /// <summary>
    /// this is the type of Helios object that supports input and output bindings
    /// 
    /// it inherits the capability of being in a profile and supporting the undo manager for all property changes
    /// </summary>
    public abstract class HeliosObject : NotificationObject
    {
        private string _name;
        private bool _designMode;
        private WeakReference _profile = new WeakReference(null);
        private int _bypassCount;
#if DEVELOPMENT_CONFIGURATION
        internal bool _tracing = false;
#endif

        protected HeliosObject(string name)
        {
            _name = name;
            OutputBindings.CollectionChanged += Outputs_CollectionChanged;
        }

        #region Properties

        /// <summary>
        /// Returns the profile that this visual is a part of
        /// </summary>
        public HeliosProfile Profile
        {
            get => _profile.Target as HeliosProfile;
            set 
            {
                HeliosProfile oldProfile = _profile.Target as HeliosProfile;
                _profile = new WeakReference(value);
                OnProfileChanged(oldProfile);
            }
        }

        public abstract string TypeIdentifier
        {
            get;
        }

        /// <summary>
        /// Gets the flag to bypass trigger events.  When this
        /// is set to true no triggers should be fired.
        /// </summary>
        public bool BypassTriggers => (_bypassCount > 0 || DesignMode);

        /// <summary>
        /// Returns the internal collection of Action descriptors used
        /// to respond to the ActionDescriptors.  Sub-classes should
        /// use this to populate their actions.
        /// </summary>
        public HeliosActionCollection Actions { get; } = new HeliosActionCollection();

        /// <summary>
        /// Returns the internal collection of Trigger descriptors used
        /// to respond to the ActionDescriptors.  Sub-classes should
        /// use this to populate their actions.
        /// </summary>
        public HeliosTriggerCollection Triggers { get; } = new HeliosTriggerCollection();

        /// <summary>
        /// Returns the internal collection of Value descriptors used
        /// to respond to the ActionDescriptors.  Sub-classes should
        /// use this to populate their actions.
        /// </summary>
        public HeliosValueCollection Values { get; } = new HeliosValueCollection();

        /// <summary>
        /// Collection of bindings which execute actions of this object.
        /// </summary>
        public HeliosBindingCollection InputBindings { get; } = new HeliosBindingCollection();

        /// <summary>
        /// Collection of bindings which this object triggers.
        /// </summary>
        public HeliosBindingCollection OutputBindings { get; } = new HeliosBindingCollection();

        /// <summary>
        /// Name of this profile object.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if ((_name != null || value == null) && (_name == null || _name.Equals(value)))
                {
                    return;
                }

                string oldName = _name;
                _name = value;
                OnPropertyChanged("Name", oldName, value, true);

                // manually push name change to our actions, triggers, values, and bindings, because we don't want to
                // reference them all with event handler subscriptions
                foreach (INamedBindingElement element in Actions.OfType<INamedBindingElement>()
                    .Concat(Triggers.OfType<INamedBindingElement>())
                    .Concat(Values.OfType<INamedBindingElement>())
                    .Concat(InputBindings)
                    .Concat(OutputBindings))
                {
                    element.RecalculateName();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this control is currently being displayed in the editor.
        /// </summary>
        public virtual bool DesignMode
        {
            get => _designMode || (Profile != null && Profile.DesignMode);
            set
            {
                if (_designMode != value)
                {
                    _designMode = value;
                    OnPropertyChanged("DesignMode", !value, value, false);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disconnects an object bindings.  This should be called on any object
        /// being removed from the profile.
        /// </summary>
        public virtual void DisconnectBindings()
        {
            foreach (HeliosBinding binding in OutputBindings)
            {
                binding.Action.Target.InputBindings.Remove(binding);
            }

            foreach (HeliosBinding binding in InputBindings)
            {
                binding.Trigger.Source.OutputBindings.Remove(binding);
            }
        }

        /// <summary>
        /// Reconnects any orphaned bindings of an object.  This should be called on any object
        /// added to the profile.
        /// </summary>
        public virtual void ReconnectBindings()
        {
            if (Profile == null)
            {
                return;
            }

            foreach (HeliosBinding binding in OutputBindings)
            {
                if (binding.Action.Target.Profile == Profile && !binding.Action.Target.InputBindings.Contains(binding))
                {
                    binding.Action.Target.InputBindings.Add(binding);
                }
            }

            foreach (HeliosBinding binding in InputBindings)
            {
                if (binding.Trigger.Source.Profile == Profile && !binding.Trigger.Source.OutputBindings.Contains(binding))
                {
                    binding.Trigger.Source.OutputBindings.Add(binding);
                }
            }
        }

        /// <summary>
        /// Notification method for profile changes.
        /// </summary>
        protected virtual void OnProfileChanged(HeliosProfile oldProfile)
        {
            // no code
        }

        /// <summary>
        /// Method to indicate we are begining a unit of work which should not fire any triggers.
        /// </summary>
        public void BeginTriggerBypass(bool bypassTriggers)
        {
            if (bypassTriggers)
            {
                _bypassCount++;
            }
        }

        /// <summary>
        /// Method to indicate we are finished with a unit of work which should not fire any triggers.
        /// </summary>
        public void EndTriggerBypass(bool bypassTriggers)
        {
            if (bypassTriggers)
            {
                _bypassCount--;
            }
        }

        /// <summary>
        /// Resets this object to default state.
        /// </summary>
        public virtual void Reset()
        {
            // Default No-Op
        }

        /// <summary>
        /// Called to read any XML content if the object is not instantiated from an
        /// empty element in <tag/> form
        /// 
        /// WARNING: Name property has not been set when this is called
        /// </summary>
        /// <param name="reader"></param>
        public abstract void ReadXml(XmlReader reader);

        public abstract void WriteXml(XmlWriter writer);

        #endregion

        void Outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (HeliosBinding binding in e.NewItems)
                {
                    binding.Trigger.TriggerFired -= binding.OnTriggerFired;
                    binding.Trigger.TriggerFired += binding.OnTriggerFired;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (HeliosBinding binding in e.OldItems)
                {
                    binding.Trigger.TriggerFired -= binding.OnTriggerFired;
                }
            }
        }

    }
}
