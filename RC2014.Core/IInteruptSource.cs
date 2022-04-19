namespace RC2014.Core
{

    public interface INonMaskableInteruptSource
    {
        event EventHandler NmiInterruptPulse;
    }

    public interface IInterruptSource
    {
        public event EventHandler InterruptPulse;
    }
}