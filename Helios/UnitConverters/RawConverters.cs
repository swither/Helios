using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.UnitConverters
{
    public class RawConverter<T>: BindingValueUnitConverter
    {
        static protected readonly bool _rawConversionsAllowed = ConfigManager.SettingsManager.LoadSetting<bool>("ProfileManager", "AllowRawConversion", true);

        public override bool IsRaw => true;

        public override bool CanConvert(BindingValueUnit from, BindingValueUnit to)
        {
            if (!GlobalOptions.HasAllowRawConversion)
            {
                return false;
            }
            if (from is Units.NumericUnit && to is T)
            {
                return true;
            }
            if (to is Units.NumericUnit && from is T)
            {
                return true;
            }
            return false;
        }

        public override BindingValue Convert(BindingValue value, BindingValueUnit from, BindingValueUnit to)
        {
            // make a new bindingvalue object just so we act like other converters,
            // in case some code relies on it somewhere
            return new BindingValue(value.DoubleValue);
        }
    }

    // now create all the raw converters as applicable
    [HeliosUnitConverter]
    public class RawAreaConverter : RawConverter<Units.AreaUnit> {}
    [HeliosUnitConverter]
    public class RawDistanceConverter : RawConverter<Units.DistanceUnit> { }
    [HeliosUnitConverter]
    public class RawMassFlowConverter : RawConverter<Units.MassFlowUnit> { }
    [HeliosUnitConverter]
    public class RawPressureConverter : RawConverter<Units.PressureUnit> { }
    [HeliosUnitConverter]
    public class RawDegreesConverter : RawConverter<Units.DegreesUnit> { }
    [HeliosUnitConverter]
    public class RawRadiansConverter : RawConverter<Units.RadiansUnit> { }
    [HeliosUnitConverter]
    public class RawSpeedConverter : RawConverter<Units.SpeedUnit> { }
    [HeliosUnitConverter]
    public class RawTimeConverter : RawConverter<Units.TimeUnit> { }
}
