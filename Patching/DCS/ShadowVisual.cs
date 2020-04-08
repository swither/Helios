using System;
using System.Collections.Generic;
using System.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class ShadowViewportEventArgs : EventArgs
    {
        public ShadowVisual Data { get; }

        public ShadowViewportEventArgs(ShadowVisual shadow)
        {
            Data = shadow;
        }
    }

    public class RawViewportEventArgs : EventArgs
    {
        public HeliosVisual Raw { get; }

        public RawViewportEventArgs(HeliosVisual visual)
        {
            Raw = visual;
        }
    }

    /// <summary>
    /// callbacks from objects shadowing visuals (viewports, monitors) in the Helios Profile
    /// to implement our model
    /// </summary>
    public interface IShadowVisualParent
    {
        Vector GlobalOffset { get; }
        double Scale { get; }
        void AddViewport(ShadowVisual viewport);
        void RemoveViewport(ShadowVisual viewport);
    }

    /// <summary>
    /// This class represents a Helios visual being observed.
    /// It is the model class.
    /// </summary>
    public class ShadowVisual : NotificationObject
    {
        public event EventHandler<RawViewportEventArgs> ViewportChanged;
        public event EventHandler<RawMonitorEventArgs> MonitorChanged;

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
        /// the monitor on which this visual is placed
        /// </summary>
        protected Monitor _monitor;

        public bool IsViewport => _viewport != null;

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
                _parent.AddViewport(this);
            }

            // observe changes
            monitor.Moved += Monitor_Modified;
            monitor.Resized += Monitor_Modified;
            visual.Moved += Visual_Modified;
            visual.Resized += Visual_Modified;
            visual.Children.CollectionChanged += Visual_Children_CollectionChanged;

            // NOTE: changes in monitor children tracked in ShadowMonitor

            if (recurse)
            {
                Instrument(monitor, visual);
            }
        }

        private void Monitor_Modified(object sender, EventArgs e)
        {
            MonitorChanged?.Invoke(this, new RawMonitorEventArgs(_monitor));
        }

        private void Visual_Modified(object sender, EventArgs e)
        {
            OnVisualModified();
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

        private void Visual_Children_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        protected virtual void OnVisualModified()
        {
            if (!IsViewport)
            {
                return;
            }

            // we actually care about this change
            ViewportChanged?.Invoke(this, new RawViewportEventArgs(_visual));
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