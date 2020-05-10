namespace net.derammo.Helios.SampleProgram.Sample
{
    /// <summary>
    /// this constructor is only used to create DesignData in the XAML Designer
    /// </summary>
    public partial class SampleViewModel
    {
        public SampleViewModel()
            : base(new SampleModel(true))
        {
            SomeViewState = 2;
            SomeOtherViewState = "Sample Data";
        }
    }
}