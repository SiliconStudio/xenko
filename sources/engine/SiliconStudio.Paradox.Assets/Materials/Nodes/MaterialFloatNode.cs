// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialFloatNode>))]
    [DataContract("MaterialFloatNode")]
    public class MaterialFloatNode : MaterialConstantNode<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloatNode"/> class.
        /// </summary>
        public MaterialFloatNode()
            : this(0.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloatNode"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaterialFloatNode(float value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Float";
        }
    }
}
