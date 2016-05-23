using System;

namespace SiliconStudio.Xenko.Engine.Events
{
    /// <summary>
    /// Options related to EventReceiver
    /// might be extended in the future
    /// </summary>
    [Flags]
    public enum EventReceiverOptions
    {
        /// <summary>
        /// If no flags are present only the most recent event will be buffered
        /// </summary>
        None,
        /// <summary>
        /// If this flag is present the events will be buffered into a queue,
        /// receivers might decide to consume at any pace they wish as long as they consume them at some point
        /// </summary>
        Buffered = 1 << 0
    }
}
