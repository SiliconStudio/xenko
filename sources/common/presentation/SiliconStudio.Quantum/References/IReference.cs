// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Quantum.References
{
    public interface IReference : IEquatable<IReference>
    {
        /// <summary>
        /// Gets the data object targeted by this reference, if available.
        /// </summary>
        object ObjectValue { get; }
    }
}
