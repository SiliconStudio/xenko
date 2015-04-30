// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Rendering
{
    public struct ModelNodeTransformation
    {
        public int ParentIndex;

        public TransformTRS Transform;

        public Matrix LocalMatrix;

        public Matrix WorldMatrix;

        /// <summary>
        /// The flags of this node.
        /// </summary>
        public ModelNodeFlags Flags;

        internal bool RenderingEnabledRecursive;
    }
}