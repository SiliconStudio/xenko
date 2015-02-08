// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Stores transformation in a TRS format (Translation, Rotation and Scaling).
    /// </summary>
    /// <remarks>
    /// It first applies scaling, then rotation, then translation.
    /// Rotation is stored in a Quaternion so that animation system can provides smooth rotation interpolations and blending.
    /// </remarks>
    [DataContract]
    public struct TransformTRS
    {
        /// <summary>
        /// The translation.
        /// </summary>
        public Vector3 Translation;

        /// <summary>
        /// The rotation.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// The scaling
        /// </summary>
        public Vector3 Scaling;
    }
}