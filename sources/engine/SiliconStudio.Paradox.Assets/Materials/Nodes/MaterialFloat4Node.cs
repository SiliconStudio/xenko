// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialFloat4Node>))]
    [DataContract("MaterialFloat4Node")]
    public class MaterialFloat4Node : MaterialConstantNode<Vector4>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloat4Node"/> class.
        /// </summary>
        public MaterialFloat4Node()
            : this(Vector4.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloat4Node"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaterialFloat4Node(Vector4 value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Float4";
        }
    }
}
