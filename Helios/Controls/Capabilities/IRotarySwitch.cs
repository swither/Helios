namespace GadrocsWorkshop.Helios.Controls.Capabilities
{
    public interface IRotarySwitch: IPulsedControl
    {
        /// <summary>
        /// the current position, in the range MinPosition..MaxPosition (inclusive)
        /// and which can be changed by adding or subtracting via Pulse(...)
        /// </summary>
        int CurrentPosition { get; }

        /// <summary>
        /// minimum position number of the switch
        /// </summary>
        int MinPosition { get; }

        /// <summary>
        /// maximum position number of the switch
        /// </summary>
        int MaxPosition { get; }

        /// <summary>
        /// true if this switch wraps around
        /// </summary>
        bool IsContinuous { get; }
    }
}
