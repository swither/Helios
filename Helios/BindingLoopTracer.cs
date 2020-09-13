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

namespace GadrocsWorkshop.Helios
{
    public class BindingLoopTracer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly System.Collections.Generic.HashSet<string> _tracesReported = new System.Collections.Generic.HashSet<string>();
        private object _tracingSource;
        private bool _traceLoop;

        internal void TraceTriggerFired(HeliosBinding heliosBinding)
        {
            // NOTE: deliberately crash if any of these are null
            HeliosObject target = heliosBinding.Action.Target;

            // IsTracing indicates a soft loop, IsExecuting indicates a hard loop
            if (target.IsTracing || heliosBinding.IsExecuting)
            {
                if (!_tracesReported.Contains(heliosBinding.Description))
                {
                    string loopType = heliosBinding.IsExecuting ? "Hard" : "Soft";
                    Logger.Warn($"{loopType} binding loop detected; Object {target.Name} may have triggered itself");
                    _traceLoop = true;
                    _tracesReported.Add(heliosBinding.Description);
                }

                _tracingSource = target;
            }
            HeliosObject source = (heliosBinding.Trigger).Source;
            source.IsTracing = true;
        }

        internal void EndTraceTriggerFired(HeliosBinding heliosBinding)
        {
            if (_traceLoop)
            {
                // log every node traversed while returning back through the sources
                // after detecting a loop
                Logger.Info($"  binding loop includes {heliosBinding.Description}");
            }

            HeliosObject source = heliosBinding.Trigger.Source;
            if (!source.IsTracing)
            {
                // not part of a loop being traced
                return;
            }

            // returning back through the sources
            source.IsTracing = false;
            if (_tracingSource != source)
            {
                // not the source currently being traced
                return;
            }

            if (_traceLoop)
            {
                // found source of loop, finish trace
                _traceLoop = false;
                Logger.Info("  binding loop trace complete");
            }

            // not tracing any more
            _tracingSource = null;
        }
    }
}