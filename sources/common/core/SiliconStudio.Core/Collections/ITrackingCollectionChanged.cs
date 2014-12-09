using System;

namespace SiliconStudio.Core.Collections
{
    public interface ITrackingCollectionChanged
    {
        /// <summary>
        /// Occurs when [collection changed].
        /// </summary>
        /// Called as is when adding an item, and in reverse-order when removing an item.
        event EventHandler<TrackingCollectionChangedEventArgs> CollectionChanged;
    }
}