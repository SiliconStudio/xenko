// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Gives the ability to control how parent matrix is computed in a <see cref="TransformComponent"/>.
    /// </summary>
    public abstract class TransformLink
    {
        /// <summary>
        /// Compute a world matrix this link represents.
        /// </summary>
        /// <param name="recursive"></param>
        /// <param name="matrix">The computed world matrix.</param>
        public abstract void ComputeMatrix(bool recursive, out Matrix matrix);
    }
}
