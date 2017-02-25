// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
