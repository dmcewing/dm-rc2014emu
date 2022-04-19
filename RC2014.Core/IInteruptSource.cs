namespace RC2014.Core
{
    public interface INonMaskableInteruptSource
    {
    }

    public interface IInterruptSource
    {
        bool IntLineIsActive { get; }
        byte? ValueOnDataBus { get; }
    }
}