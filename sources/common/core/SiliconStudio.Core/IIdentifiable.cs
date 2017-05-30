// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Base interface for all identifiable instances.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Gets the id of this instance
        /// </summary>
        [NonOverridable]
        Guid Id { get; set; }
    }
}
