// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialColorNode>))]
    [DataContract("MaterialColorNode")]
    public class MaterialColorNode : MaterialConstantNode<Color4>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialColorNode"/> class.
        /// </summary>
        public MaterialColorNode()
            : this(Color4.Black)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialColorNode"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaterialColorNode(Color4 value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Color";
        }
    }
}
