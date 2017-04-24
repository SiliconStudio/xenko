// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract]
    public struct ModelNodeTransformation
    {
        public int ParentIndex;

        public TransformTRS Transform;

        public Matrix LocalMatrix;

        public Matrix WorldMatrix;

        public bool IsScalingNegative;

        /// <summary>
        /// The flags of this node.
        /// </summary>
        public ModelNodeFlags Flags;

        internal bool RenderingEnabledRecursive;
    }
}
