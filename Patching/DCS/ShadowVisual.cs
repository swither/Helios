using System;
using System.Collections.Generic;
using System.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    // callbacks from shadow objects
    public interface IShadowVisualParent
    {
        Vector GlobalOffset { get; }
        double Scale { get; }
        void AddViewport(ShadowVisual viewport);
        void RemoveViewport(ShadowVisual viewport);
        void ChangeViewport(ShadowVisual viewport);
        void ChangeMonitor(ShadowMonitor shadowMonitor);
        void ChangeMonitorKey(ShadowMonitor shadowMonitor, string oldKey, string newKey);
    }

    public class ShadowVisual
    {
        /// <summary>
        /// callbacks to our parent
        /// </summary>
        protected IShadowVisualParent _parent;

        /// <summary>
        /// the visual we are shadowing
        /// </summary>
        protected HeliosVisual _visual;

        /// <summary>
        /// map from visual child to its shadow representation
        /// </summary>
        protected Dictionary<HeliosVisual, ShadowVisual> _children = new Dictionary<HeliosVisual, ShadowVisual>();

        /// <summary>
        /// viewport extent or null if this item is not a viewport
        /// </summary>
        protected IViewportExtent _viewport;

        /// <summary>
        /// viewport view model or null if this item is not a viewport
        /// </summary>
        protected ViewportViewModel _viewportViewModel;

        /// <summary>
        /// the monitor on which this visual is placed
        /// </summary>
        protected Monitor _monitor;

        public bool IsViewport => _viewport != null;

        public ViewportViewModel ViewportViewModel => _viewportViewModel;

        public Dictionary<HeliosVisual, ShadowVisual> Children => _children;

        public HeliosVisual Visual => _visual;

        public Monitor Monitor => _monitor;

        public IViewportExtent Viewport => _viewport;

        internal ShadowVisual(IShadowVisualParent parent, Monitor monitor, HeliosVisual visual, bool recurse = true)
        {
            // create a shadow object
            _parent = parent;
            _monitor = monitor;
            _visual = visual;
            _viewport = visual as IViewportExtent;
            if (IsViewport)
            {
                _viewportViewModel = new ViewportViewModel(_monitor, _parent.GlobalOffset, visual, _parent.Scale);
                _parent.AddViewport(this);
            }

            // observe changes
            visual.Moved += Visual_Modified;
            visual.Resized += Visual_Modified;
            visual.Children.CollectionChanged += Visual_Children_CollectionChanged;

            if (recurse)
            {
                Instrument(monitor, visual);
            }
        }

        private void Visual_Modified(object sender, EventArgs e)
        {
            OnModified();
        }

        /// <summary>
        /// recusively add all descendants for tracking
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="visual"></param>
        protected void Instrument(Monitor monitor, HeliosVisual visual)
        {
            foreach (HeliosVisual child in visual.Children)
            {
                _children[child] = new ShadowVisual(_parent, monitor, child);
            }
        }

        private void Visual_Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (HeliosVisual newChild in e.NewItems)
                {
                    OnAdded(newChild);
                }
            }
            if (e.OldItems != null)
            {
                foreach (HeliosVisual oldChild in e.OldItems)
                {
                    OnRemoved(oldChild);
                }
            }
        }

        protected virtual void OnAdded(HeliosVisual newChild)
        {
            _children[newChild] = new ShadowVisual(_parent, _monitor, newChild);
        }

        protected virtual void OnRemoved(HeliosVisual oldChild)
        {
            ShadowVisual shadow = _children[oldChild];
            shadow.Dispose();
            _children.Remove(oldChild);
        }

        protected virtual void OnModified()
        {
            if (IsViewport)
            {
                // we actually care about this change
                _viewportViewModel.Update(_visual);
                _parent.ChangeViewport(this);
            }
        }

        internal virtual void Dispose()
        {
            if (IsViewport)
            {
                _parent.RemoveViewport(this);
            }
            _visual.Moved -= Visual_Modified;
            _visual.Resized -= Visual_Modified;
            _visual.Children.CollectionChanged -= Visual_Children_CollectionChanged;
            foreach (ShadowVisual child in _children.Values)
            {
                child.Dispose();
            }
        }
    }
}
