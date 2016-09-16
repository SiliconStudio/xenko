namespace SiliconStudio.Core.MicroThreading
{
    internal interface IMicroThreadSynchronizationContext
    {
        MicroThread MicroThread { get; }
    }
}
