// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core
{
    /// <summary>
    /// Interface ICollectorHolder for an instance that can collect other instance.
    /// </summary>
    public interface ICollectorHolder
    {
        /// <summary>
        /// Gets the collector.
        /// </summary>
        /// <value>The collector.</value>
        ObjectCollector Collector { get; }
    }
}

