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

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using GadrocsWorkshop.Helios.Controls.Capabilities;

namespace GadrocsWorkshop.Helios.Interfaces.ControlRouter
{
    public class ControlRouterPort : NotificationObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// the persisted name of this port in case some ports are deleted later, this will
        /// keep bindings working
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }

        internal HeliosActionCollection Actions { get; } = new HeliosActionCollection();
        internal HeliosTriggerCollection Triggers { get; } = new HeliosTriggerCollection();
        internal HeliosValueCollection Values { get; } = new HeliosValueCollection();

        // currently bound control info
        private HeliosValue _controlName;

        /// <summary>
        /// backing field for property PulsesPerRevolution, contains
        /// the number of pulses that the input control generates per revolution
        /// </summary>
        private double _pulsesPerRevolution = 72d;

        /// <summary>
        /// backing field for property PulsesPerDetent, contains
        /// the number of pulses that the input control generates per detent on the input control
        /// </summary>
        private double _pulsesPerDetent = 4d;

        /// <summary>
        /// backing field for property PulseSwitches, contains
        /// true if switches should be incremented by one position per pulse instead of by converting to angles
        /// </summary>
        private bool _pulseSwitches;

        /// <summary>
        /// backing field for property PulseAll, contains
        /// true if all pulse-capable controls should be incremented by one position per pulse instead of by converting to angles
        /// </summary>
        private bool _pulseAll;

        /// <summary>
        /// backing field for property ValuePerRevolution, contains
        /// the value that would be sent by the input control if it was positioned at an angle of 360 degrees
        /// </summary>
        private double _valuePerRevolution = 1d;

        /// <summary>
        /// backing field for property ValueAtZeroDegrees, contains
        /// the value that would be sent by the input control if it was positioned at an angle of 0 degrees
        /// </summary>
        private double _valueAtZeroDegrees;

        // back reference to parent
        private ControlRouter _parent;

        // control interfaces we know how to use, set to non-null if bound to a control that supports them
        private IRotaryControl _boundRotaryControl;
        private IPulsedControl _boundPulsedControl;

        // our target control's angle at time of binding
        private double _initialAngle;

        // our input value at time of binding
        private BindingValue _initialInputValue;
        
        // the angle to which we set the bound control on the last update
        private double _lastAngle;

        // pulses that are not yet delivered, because we are accumulating them
        private double _undeliveredPulses;

        // just for debugging purposes, track how many pulses we have received
        private double _pulsesSinceBinding;

        /// <summary>
        /// true if the port is currently sending pulses directly to the target instead of converting to angles
        /// </summary>
        private bool _pulseMode;

        /// <summary>
        /// if the bound control moves more than this many degrees without us doing it,
        /// we assume some other control has moved it and re-bind to the control so we don't
        /// suddenly snap it to our input value
        /// </summary>
        private const double CONTROL_ANGLE_TOLERANCE = 0.1d;

        // deserialization constructor
        public ControlRouterPort()
        {
            // no code
        }

        /// <summary>
        /// constructor for newly created port
        /// </summary>
        /// <param name="name"></param>
        public ControlRouterPort(string name) : this()
        {
            Name = name;
        }

        internal void Initialize(ControlRouter parent)
        {
            _parent = parent;

            // conversion from pulses to values
            HeliosAction changeValueByPulses = new HeliosAction(parent, Name, "Value from Pulses", "change",
                "Increment of decrement the value by a number of pulses", "+/- pulses", BindingValueUnits.Numeric);
            changeValueByPulses.Execute += ChangeValueByPulses_Execute;
            Actions.Add(changeValueByPulses);

            // direct angle mapping
            HeliosAction changeAngle = new HeliosAction(parent, Name, "Relative Angle", "change",
                "Control the angle of bound rotary control relative to where it was when it was bound",
                "Value of Control", BindingValueUnits.Numeric);
            changeAngle.Execute += ChangeAngle_Execute;
            Actions.Add(changeAngle);

            // status display: name of bound control
            _controlName = new HeliosValue(parent, BindingValue.Empty, Name, "Bound Control",
                "The name of the control receiving the output from this port", "Control name from Profile",
                BindingValueUnits.Text);
            Values.Add(_controlName);
            Triggers.Add(_controlName);
        }

        /// <summary>
        /// called on profile reset
        /// </summary>
        internal void Reset()
        {
            _boundRotaryControl = null;
            _pulsesSinceBinding = 0d;
            _lastAngle = 0d;
            _initialInputValue = null;
        }

        private void ChangeValueByPulses_Execute(object action, HeliosActionEventArgs e)
        {
            ClaimControlIfAvailable(e);
            if (Math.Abs(e.Value.DoubleValue) < 0.1)
            {
                // this represents no change
                return;
            }

            Logger.Debug("received change by pulses of {Pulses}", e.Value.DoubleValue);

            // track total change
            _pulsesSinceBinding += e.Value.DoubleValue;

            if (_boundRotaryControl != null && !_pulseMode)
            {
                // convert to angle
                _boundRotaryControl.ControlAngle += e.Value.DoubleValue * 360d / _pulsesPerRevolution;
                Logger.Debug("after {Pulses} pulses, set control to {Angle}", _pulsesSinceBinding, _boundRotaryControl.ControlAngle);
                return;
            }

            if (_boundPulsedControl != null)
            {
                // deliver as pulses
                int discretePulses;

                // this functionality does not work with < 1 pulse per detent, so eliminate that case along with zero division
                if (_pulsesPerDetent > 1d)
                {
                    // remainder from previous pulses, including negative remainders
                    _undeliveredPulses += e.Value.DoubleValue / _pulsesPerDetent;
                    if (_undeliveredPulses >= 1d)
                    {
                        discretePulses = (int) _undeliveredPulses;
                        Logger.Debug("accumulated {Pulses} detents, sending {DiscretePulses} to increment", _undeliveredPulses, discretePulses);
                        _undeliveredPulses %= 1d;
                    }
                    else if (_undeliveredPulses <= -1d)
                    {
                        discretePulses = (int) _undeliveredPulses;
                        Logger.Debug("accumulated {Pulses} detents, sending {DiscretePulses} to decrement", _undeliveredPulses, discretePulses);
                        _undeliveredPulses %= 1d;
                    }
                    else
                    {
                        discretePulses = 0;
                    }
                }
                else
                {
                    discretePulses = (int) e.Value.DoubleValue;
                }

                if (discretePulses != 0)
                {
                    _boundPulsedControl.Pulse(discretePulses);
                }
                return;
            }

            Logger.Debug("no supported interfaces on target control; cannot deliver pulses");
        }

        private void ChangeAngle_Execute(object action, HeliosActionEventArgs e)
        {
            ClaimControlIfAvailable(e);
            Logger.Debug("received relative angle {Degrees}", e.Value.DoubleValue);
            if (_boundRotaryControl == null)
            {
                return;
            }

            // if the control has moved from where we last set it, then re-bind
            // don't just track deltas in this control because then error accumulates
            double currentAngle = _boundRotaryControl.ControlAngle;
            if (Math.Abs(_lastAngle - currentAngle) > CONTROL_ANGLE_TOLERANCE)
            {
                // some other control has moved it, rebind at this position
                Logger.Debug("another control has moved our bound control, we will now process relative angle changes to this new starting position {Degrees}", currentAngle);
                _initialAngle = currentAngle;
                _lastAngle = _initialAngle;
                _initialInputValue = e.Value;
                return;
            }

            // division by zero guard and unreasonable scale guard
            double scale = ValuePerRevolution;
            if (Math.Abs(scale) < 0.00001)
            {
                scale = 0.00001;
            }

            // calculate absolute position to avoid accumulating error
            _boundRotaryControl.ControlAngle = _initialAngle +
                                               (e.Value.DoubleValue -
                                                (ValueAtZeroDegrees + _initialInputValue.DoubleValue)) * 360.0 / scale;

            // read back what it actually did
            _lastAngle = _boundRotaryControl.ControlAngle;
        }

        private void ClaimControlIfAvailable(HeliosActionEventArgs e)
        {
            if (!_parent.TryClaimControl(out INamedControl control))
            {
                return;
            }

            _boundRotaryControl = control as IRotaryControl;
            _boundPulsedControl = control as IPulsedControl;
            _controlName.SetValue(new BindingValue(control.Name), false);
            _initialAngle = _boundRotaryControl?.ControlAngle ?? 0d;
            _lastAngle = _initialAngle;
            _initialInputValue = e.Value;
            _pulsesSinceBinding = 0d;
            _pulseMode = PulseAll || (PulseSwitches && (control is IRotarySwitch));
        }

        /// <summary>
        /// the number of pulses that the input control generates per revolution
        /// </summary>
        [DefaultValue(72d)]
        [XmlElement("PulsesPerRevolution")]
        public double PulsesPerRevolution
        {
            get => _pulsesPerRevolution;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator we really do want to process every change
                if (_pulsesPerRevolution == value)
                {
                    return;
                }

                double oldValue = _pulsesPerRevolution;
                _pulsesPerRevolution = value;
                OnPropertyChanged("PulsesPerRevolution", oldValue, value, true);
            }
        }

        /// <summary>
        /// the number of pulses that the input control generates per detent on the input control
        /// </summary>
        [DefaultValue(4d)]
        [XmlElement("PulsesPerDetent")]
        public double PulsesPerDetent
        {
            get => _pulsesPerDetent;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator we really do want to process every change
                if (_pulsesPerDetent == value)
                {
                    return;
                }

                double oldValue = _pulsesPerDetent;
                _pulsesPerDetent = value;
                OnPropertyChanged("PulsesPerDetent", oldValue, value, true);
            }
        }

        /// <summary>
        /// true if switches should be incremented by one position per pulse instead of by converting to angles
        /// </summary>
        [DefaultValue(false)]
        [XmlElement("PulseSwitches")]
        public bool PulseSwitches
        {
            get => _pulseSwitches;
            set
            {
                if (_pulseSwitches == value)
                {
                    return;
                }
                bool oldValue = _pulseSwitches;
                _pulseSwitches = value;
                OnPropertyChanged("PulseSwitches", oldValue, value, true);
            }
        }

        /// <summary>
        /// true if all pulse-capable controls should be incremented by one position per pulse instead of by converting to angles
        /// </summary>
        [DefaultValue(false)]
        [XmlElement("PulseAll")]
        public bool PulseAll
        {
            get => _pulseAll;
            set
            {
                if (_pulseAll == value)
                {
                    return;
                }
                bool oldValue = _pulseAll;
                _pulseAll = value;
                OnPropertyChanged("PulseAll", oldValue, value, true);
            }
        }

        /// <summary>
        /// the value that would be sent by the input control if it was positioned at an angle of 0 degrees
        /// </summary>
        [DefaultValue(0d)]
        [XmlElement("ValueAtZeroDegrees")]
        public double ValueAtZeroDegrees
        {
            get => _valueAtZeroDegrees;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator we really do want to process every change
                if (_valueAtZeroDegrees == value)
                {
                    return;
                }

                double oldValue = _valueAtZeroDegrees;
                _valueAtZeroDegrees = value;
                OnPropertyChanged("ValueAtZeroDegrees", oldValue, value, true);
            }
        }

        /// <summary>
        /// the value that would be sent by the input control if it was positioned at an angle of 360 degrees
        /// </summary>
        [DefaultValue(1d)]
        [XmlElement("ValuePerRevolution")]
        public double ValuePerRevolution
        {
            get => _valuePerRevolution;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator we really do want to process every change
                if (_valuePerRevolution == value)
                {
                    return;
                }

                double oldValue = _valuePerRevolution;
                _valuePerRevolution = value;
                OnPropertyChanged("ValuePerRevolution", oldValue, value, true);
            }
        }
    }
}