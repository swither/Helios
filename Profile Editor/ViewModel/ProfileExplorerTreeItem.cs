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

using System.Collections;
using System.Linq;

namespace GadrocsWorkshop.Helios.ProfileEditor.ViewModel
{
    using GadrocsWorkshop.Helios;
    using GadrocsWorkshop.Helios.Controls;
    using System;

    public class ProfileExplorerTreeItem : NotificationObject
    {
        private WeakReference _parent;

        private readonly ProfileExplorerTreeItemType _includeTypes;

        private string _name;
        private string _description;
        private bool _isSelected;
        private bool _isExpanded;

        public ProfileExplorerTreeItem(HeliosProfile profile, ProfileExplorerTreeItemType includeTypes)
            : this(profile.Name, "", null, includeTypes)
        {
            ContextItem = profile;
            ItemType = ProfileExplorerTreeItemType.Profile;

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Monitor))
            {
                ProfileExplorerTreeItem monitors = new ProfileExplorerTreeItem("Monitors", profile.Monitors, this, includeTypes);
                if (monitors.HasChildren)
                {
                    Children.Add(monitors);
                }
                profile.Monitors.CollectionChanged += Monitors_CollectionChanged;
            }

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Interface))
            {
                ProfileExplorerTreeItem interfaces = new ProfileExplorerTreeItem("Interfaces", profile.Interfaces, this, includeTypes);
                if (interfaces.HasChildren)
                {
                    Children.Add(interfaces);
                }
                profile.Interfaces.CollectionChanged += Interfaces_CollectionChanged;
            }
        }

        public ProfileExplorerTreeItem(HeliosObject hobj, ProfileExplorerTreeItemType includeTypes)
            : this(hobj.Name, "", null, includeTypes)
        {
            ContextItem = hobj;
            ItemType = ProfileExplorerTreeItemType.Visual;

            AddChild(hobj, includeTypes);
        }

        private ProfileExplorerTreeItem(string name, string description, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
        {
            _parent = new WeakReference(parent);
            _name = name;
            _description = description;
            ItemType = ProfileExplorerTreeItemType.Folder;
            _includeTypes = includeTypes;
            Children = new ProfileExplorerTreeItemCollection();
        }

        private ProfileExplorerTreeItem(string name, HeliosInterfaceCollection interfaces, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(name, "", parent, includeTypes)
        {
            ItemType = ProfileExplorerTreeItemType.Folder;
            foreach (HeliosInterface heliosInterface in interfaces)
            {
                ProfileExplorerTreeItem item = new ProfileExplorerTreeItem(heliosInterface, this, includeTypes);
                Children.Add(item);
            }
        }

        private ProfileExplorerTreeItem(HeliosInterface heliosInterface, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(heliosInterface.Name, "", parent, includeTypes)
        {
            ItemType = ProfileExplorerTreeItemType.Interface;
            ContextItem = heliosInterface;

            AddChild(heliosInterface, includeTypes);
        }

        private ProfileExplorerTreeItem(string name, MonitorCollection monitors, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(name, "", parent, includeTypes)
        {
            ItemType = ProfileExplorerTreeItemType.Folder;
            foreach (Monitor monitor in monitors)
            {
                ProfileExplorerTreeItem monitorItem = new ProfileExplorerTreeItem(monitor, this, includeTypes);
                Children.Add(monitorItem);
            }
        }

        private ProfileExplorerTreeItem(HeliosVisual visual, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(visual.Name, "", parent, includeTypes)
        {
            if (visual.GetType() == typeof(Monitor))
            {
                ItemType = ProfileExplorerTreeItemType.Monitor;
            }
            else if (visual.GetType() == typeof(HeliosPanel))
            {
                ItemType = ProfileExplorerTreeItemType.Panel;
            }
            else
            {
                ItemType = ProfileExplorerTreeItemType.Visual;
            }
            ContextItem = visual;

            AddChild(visual, includeTypes);

            foreach (HeliosVisual child in visual.Children)
            {
                if ((child is HeliosPanel && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Panel)) ||
                    (child != null && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Visual)))
                {
                    Children.Add(new ProfileExplorerTreeItem(child, this, _includeTypes));
                }
            }

            visual.Children.CollectionChanged += VisualChildren_CollectionChanged;
        }

        private ProfileExplorerTreeItem(IBindingAction item, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(item.ActionName, item.ActionDescription, parent, includeTypes)
        {
            if (!includeTypes.HasFlag(ProfileExplorerTreeItemType.Binding))
            {
                return;
            }

            ContextItem = item;
            ItemType = ProfileExplorerTreeItemType.Action;
            //SortName = item.Name + " " + item.ActionVerb;

            foreach (HeliosBinding binding in item.Owner.InputBindings)
            {
                if (binding.Action == item)
                {
                    Children.Add(new ProfileExplorerTreeItem(binding, this, includeTypes));
                }
            }
            item.Target.InputBindings.CollectionChanged += Bindings_CollectionChanged;
        }

        private ProfileExplorerTreeItem(IBindingTrigger item, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(item.TriggerName, item.TriggerDescription, parent, includeTypes)
        {
            ContextItem = item;
            ItemType = ProfileExplorerTreeItemType.Trigger;

            if (!includeTypes.HasFlag(ProfileExplorerTreeItemType.Binding))
            {
                return;
            }

            foreach (HeliosBinding binding in item.Owner.OutputBindings
                .Where(binding => binding.Trigger == item))
            {
                Children.Add(new ProfileExplorerTreeItem(binding, this, includeTypes));
            }
            item.Source.OutputBindings.CollectionChanged += Bindings_CollectionChanged;
        }

        private ProfileExplorerTreeItem(HeliosBinding item, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(item.Description, "", parent, includeTypes)
        {
            ContextItem = item;
            ItemType = ProfileExplorerTreeItemType.Binding;
            item.PropertyChanged += Binding_PropertyChanged;
        }

        public void Disconnect()
        {
            switch (ItemType)
            {
                case ProfileExplorerTreeItemType.Monitor:
                case ProfileExplorerTreeItemType.Panel:
                case ProfileExplorerTreeItemType.Visual:
                    HeliosVisual visual = (HeliosVisual)ContextItem;
                    visual.PropertyChanged -= HeliosObject_PropertyChanged;
                    visual.Children.CollectionChanged -= VisualChildren_CollectionChanged;
                    visual.Triggers.CollectionChanged += Triggers_CollectionChanged;
                    visual.Actions.CollectionChanged += Actions_CollectionChanged;
                    break;
                case ProfileExplorerTreeItemType.Interface:
                    HeliosInterface heliosInterface = (HeliosInterface)ContextItem;
                    heliosInterface.PropertyChanged -= HeliosObject_PropertyChanged;
                    heliosInterface.Triggers.CollectionChanged += Triggers_CollectionChanged;
                    heliosInterface.Actions.CollectionChanged += Actions_CollectionChanged;
                    break;
                case ProfileExplorerTreeItemType.Action:
                    IBindingAction action = (IBindingAction)ContextItem;
                    action.Target.InputBindings.CollectionChanged -= Bindings_CollectionChanged; 
                    break;
                case ProfileExplorerTreeItemType.Trigger:
                    IBindingTrigger trigger = (IBindingTrigger)ContextItem;
                    trigger.Source.OutputBindings.CollectionChanged -= Bindings_CollectionChanged;
                    break;
                case ProfileExplorerTreeItemType.Value:
                    break;
                case ProfileExplorerTreeItemType.Binding:
                    HeliosBinding binding = (HeliosBinding)ContextItem;
                    binding.PropertyChanged += Binding_PropertyChanged;
                    break;
                case ProfileExplorerTreeItemType.Profile:
                    HeliosProfile profile = (HeliosProfile)ContextItem;
                    profile.PropertyChanged -= HeliosObject_PropertyChanged;
                    profile.Interfaces.CollectionChanged -= Interfaces_CollectionChanged;
                    profile.Monitors.CollectionChanged -= Monitors_CollectionChanged;
                    break;
                case ProfileExplorerTreeItemType.Folder:
                    // no resources; children are disconnected from their resources below in tail recursion
                    break;
                default:
                    break;
            }

            foreach (ProfileExplorerTreeItem child in Children)
            {
                child.Disconnect();
            }
        }

        private void Monitors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FolderContentsChanged<Monitor>("Monitors", e.OldItems, e.NewItems,
                (monitor) => new ProfileExplorerTreeItem(monitor, this, _includeTypes));
        }

        private void Interfaces_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FolderContentsChanged<HeliosInterface>("Interfaces", e.OldItems, e.NewItems,
                (heliosInterface) => new ProfileExplorerTreeItem(heliosInterface, this, _includeTypes));
        }

        /// <summary>
        /// common code for handling collection changes to tree item collections that are represented as a named folder
        /// rather than just directly adding children under the current node
        ///
        /// this type of folder is not removed when empty
        /// </summary>
        /// <typeparam name="THeliosObject"></typeparam>
        /// <param name="folderName"></param>
        /// <param name="removedChildren"></param>
        /// <param name="addedChildren"></param>
        /// <param name="factory"></param>
        private void FolderContentsChanged<THeliosObject>(string folderName, IEnumerable removedChildren, IEnumerable addedChildren, Func<THeliosObject, ProfileExplorerTreeItem> factory)
        {
            bool newFolder = false;
            ProfileExplorerTreeItem childrenFolder;

            // lazy create folder
            if (HasFolder(folderName))
            {
                childrenFolder = GetFolder(folderName);
            }
            else
            {
                childrenFolder = new ProfileExplorerTreeItem(folderName, "", this, _includeTypes);
                newFolder = true;
            }

            if (removedChildren != null)
            {
                foreach (THeliosObject removedChild in removedChildren)
                {
                    ProfileExplorerTreeItem childItem = childrenFolder.GetChildObject(removedChild);
                    if (childItem == null)
                    {
                        // not found
                        continue;
                    }

                    // remove from folder, but never remove the folder itself, as it is considered part of the UI
                    childItem.Disconnect();
                    childrenFolder.Children.Remove(childItem);
                }
            }

            if (addedChildren != null)
            {
                foreach (THeliosObject addedChild in addedChildren)
                {
                    ProfileExplorerTreeItem childItem = factory(addedChild);
                    childrenFolder.Children.Add(childItem);
                }
            }

            if (newFolder && childrenFolder.HasChildren)
            {
                Children.Add(childrenFolder);
            }
        }

        /// <summary>
        /// common code for handling collection changes to tree item collections that directly create their child elements
        /// under the current tree node
        /// </summary>
        /// <typeparam name="THeliosObject"></typeparam>
        /// <param name="removedChildren"></param>
        /// <param name="addedChildren"></param>
        /// <param name="addingPredicate"></param>
        /// <param name="factory"></param>
        private void DirectContentsChanged<THeliosObject>(IEnumerable removedChildren, IEnumerable addedChildren,
            Func<THeliosObject, bool> addingPredicate, Func<THeliosObject, ProfileExplorerTreeItem> factory)
        {
            if (removedChildren != null)
            {
                foreach (ProfileExplorerTreeItem childItem in removedChildren
                    .OfType<THeliosObject>()
                    .Select(childItem => GetChildObject(childItem))
                    .Where(child => child != null))
                {
                    childItem.Disconnect();
                    Children.Remove(childItem);
                }
            }

            if (addedChildren != null)
            {
                Children.AddRange(addedChildren.OfType<THeliosObject>().Where(addingPredicate).Select(factory));
            }
        }

        private void VisualChildren_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DirectContentsChanged<HeliosVisual>(e.OldItems, e.NewItems, ShouldAddVisual, child => new ProfileExplorerTreeItem(child, this, _includeTypes));
        }

        private bool ShouldAddVisual(HeliosVisual visual)
        {
            return (visual is HeliosPanel && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Panel)) ||
                   (visual != null && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Visual));
        }

        private void Bindings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DirectContentsChanged<HeliosBinding>(e.OldItems, e.NewItems, ShouldAddBinding,
                child => new ProfileExplorerTreeItem(child, this, _includeTypes));
        }

        private bool ShouldAddBinding(HeliosBinding child)
        {
            if (_includeTypes.HasFlag(ProfileExplorerTreeItemType.Binding))
            {
                // we don't do bindings
                return false;
            }

            if (child.Action != ContextItem && child.Trigger != ContextItem)
            {
                // not attached to this item
                return false;
            }

            // auto expand if anything added
            IsExpanded = true;
            return true;
        }

        /// <summary>
        /// common code for handling collection changes to tree item collections that are represented as folders with items,
        /// where the folder name is dependent on the item (such as devices and their commands/values)
        /// </summary>
        /// <typeparam name="THeliosObject"></typeparam>
        /// <param name="removedChildren"></param>
        /// <param name="addedChildren"></param>
        /// <param name="folderChooser">function that returns null if the item should not be in the tree, a folder name if the item goes in a folder, and an empty string if the item should go directly under the current node</param>
        /// <param name="addItemHandler">function to call for each new item, has to handle the folder creation</param>
        private void StructuredContentsChanged<THeliosObject>(IEnumerable removedChildren, IEnumerable addedChildren,
            Func<THeliosObject, string> folderChooser, Action<THeliosObject, ProfileExplorerTreeItemType> addItemHandler)
        {
            if (removedChildren != null)
            {
                foreach (THeliosObject removedChild in removedChildren)
                {
                    string folderName = folderChooser(removedChild);
                    if (folderName == null)
                    {
                        // should not even be here
                        continue;
                    }

                    ProfileExplorerTreeItem childItem;
                    if (folderName.Length == 0)
                    {
                        childItem = GetChildObject(removedChild);
                        if (childItem == null)
                        {
                            // not found
                            continue;
                        }
                        Children.Remove(childItem);
                    }
                    else
                    {
                        ProfileExplorerTreeItem folder = GetFolder(folderName);
                        if (folder == null)
                        {
                            // folder not found
                            continue;
                        }

                        childItem = folder.GetChildObject(removedChild);
                        if (childItem == null)
                        {
                            // child not in folder
                            continue;
                        }
                        folder.Children.Remove(childItem);

                        if (folder.Children.Count == 0)
                        {
                            // remove empty
                            Children.Remove(folder);
                        }
                    }

                    // now handle child removed
                    childItem.Disconnect();
                }
            }

            if (addedChildren == null)
            {
                return;
            }

            foreach (THeliosObject addedChild in addedChildren)
            {
                // handler will do the folder creation, if any
                addItemHandler(addedChild, _includeTypes);
            }
        }

        private void Triggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StructuredContentsChanged<IBindingTrigger>(e.OldItems, e.NewItems, GetDeviceNameForUserInterface, AddTrigger);
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StructuredContentsChanged<IBindingAction>(e.OldItems, e.NewItems, GetDeviceNameForUserInterface, AddAction);
        }

        private void Binding_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Name"))
            {
                Name = ((HeliosBinding)ContextItem).Description;
            }
        }

        public void ExpandAll()
        {
            IsExpanded = true;
            foreach (ProfileExplorerTreeItem child in Children)
            {
                child.ExpandAll();
            }
        }

        #region Properties

        public bool HasChildren => Children != null && Children.Any();

        public ProfileExplorerTreeItem Parent
        {
            get => _parent.Target as ProfileExplorerTreeItem;
            private set => _parent = new WeakReference(value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if ((_name == null && value != null)
                    || (_name != null && !_name.Equals(value)))
                {
                    string oldValue = _name;
                    _name = value;
                    OnPropertyChanged("Name", oldValue, value, false);
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!_isSelected.Equals(value))
                {
                    bool oldValue = _isSelected;
                    _isSelected = value;
                    OnPropertyChanged("IsSelected", oldValue, value, false);
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (!_isExpanded.Equals(value))
                {
                    bool oldValue = _isExpanded;
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded", oldValue, value, false);
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if ((_description == null && value != null)
                    || (_description != null && !_description.Equals(value)))
                {
                    string oldValue = _description;
                    _description = value;
                    OnPropertyChanged("Description", oldValue, value, false);
                }
            }
        }

        public ProfileExplorerTreeItemType ItemType { get; }

        public ProfileExplorerTreeItemCollection Children { get; }

        public object ContextItem { get; }

        #endregion

        private void AddChild(HeliosObject hobj, ProfileExplorerTreeItemType includeTypes)
        {
            hobj.PropertyChanged += HeliosObject_PropertyChanged;

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Trigger))
            {
                foreach (IBindingTrigger trigger in hobj.Triggers)
                {
                    AddTrigger(trigger, includeTypes);
                }
                hobj.Triggers.CollectionChanged += Triggers_CollectionChanged;
            }

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Action))
            {
                foreach (IBindingAction action in hobj.Actions)
                {
                    AddAction(action, includeTypes);
                }
                hobj.Actions.CollectionChanged += Actions_CollectionChanged;
            }
        }

        private void HeliosObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Name") && ContextItem is HeliosObject obj)
            {
                Name = obj.Name;
            }
        }

        private static string GetDeviceNameForUserInterface(IBindingElement element)
        {
            if (element is IBindingElement2 element2)
            {
                return element2.DeviceInUserInterface;
            }
            return element.Device;
        }

        /// <summary>
        /// common code for Trigger and Action items
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childItem"></param>
        /// <param name="includeTypes"></param>
        private void AddBindingElement(IBindingElement child, ProfileExplorerTreeItem childItem, ProfileExplorerTreeItemType includeTypes)
        {
            if (childItem.HasChildren || includeTypes.HasFlag(ProfileExplorerTreeItemType.Trigger))
            {
                string deviceName = GetDeviceNameForUserInterface(child);
                if (deviceName.Length > 0)
                {
                    if (!HasFolder(deviceName))
                    {
                        Children.Add(new ProfileExplorerTreeItem(deviceName, "", this, includeTypes));
                    }

                    ProfileExplorerTreeItem deviceItem = GetFolder(deviceName);
                    childItem.Parent = deviceItem;
                    deviceItem.Children.Add(childItem);
                }
                else
                {
                    Children.Add(childItem);
                }
            }
            else
            {
                // let it die
                childItem.Disconnect();
            }
        }

        private void AddTrigger(IBindingTrigger trigger, ProfileExplorerTreeItemType includeTypes)
        {
            AddBindingElement(trigger, new ProfileExplorerTreeItem(trigger, this, includeTypes), includeTypes);
        }

        public void AddAction(IBindingAction action, ProfileExplorerTreeItemType includeTypes)
        {
            AddBindingElement(action, new ProfileExplorerTreeItem(action, this, includeTypes), includeTypes);
        }

        private bool HasFolder(string folderName)
        {
            return GetFolder(folderName) != null;
        }

        private ProfileExplorerTreeItem GetFolder(string folderName) => 
            Children.FirstOrDefault(child => child.Name.Equals(folderName) && child.ItemType == ProfileExplorerTreeItemType.Folder);

        // XXX investigate why this is unused, what was it used for historically?
        private bool HasChildObject(HeliosObject childObject)
        {
            return GetChildObject(childObject) != null;
        }

        private ProfileExplorerTreeItem GetChildObject(object childObject)
        {
            foreach (ProfileExplorerTreeItem child in Children)
            {
                if (child.ContextItem == childObject)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
