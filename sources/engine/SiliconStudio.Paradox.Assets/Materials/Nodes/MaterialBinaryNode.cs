// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    /// <summary>
    /// A node that describe a binary operation between two <see cref="IMaterialNode"/>
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<MaterialBinaryNode>))]
    [DataContract("MaterialBinaryNode")]
    public class MaterialBinaryNode : MaterialNodeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBinaryNode"/> class.
        /// </summary>
        public MaterialBinaryNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBinaryNode"/> class.
        /// </summary>
        /// <param name="leftChild">The left child.</param>
        /// <param name="rightChild">The right child.</param>
        /// <param name="materialBinaryOperand">The material binary operand.</param>
        public MaterialBinaryNode(IMaterialNode leftChild, IMaterialNode rightChild, MaterialBinaryOperand materialBinaryOperand)
        {
            LeftChild = leftChild;
            RightChild = rightChild;
            Operand = materialBinaryOperand;
        }

        /// <summary>
        /// The operation to blend the nodes.
        /// </summary>
        /// <userdoc>
        /// The operation between the background (LeftChild) and the foreground (RightChild).
        /// </userdoc>
        [DataMember(10)]
        public MaterialBinaryOperand Operand { get; set; }

        /// <summary>
        /// The left (background) child node.
        /// </summary>
        /// <userdoc>
        /// The background color mapping.
        /// </userdoc>
        [DataMember(20)]
        public IMaterialNode LeftChild { get; set; }

        /// <summary>
        /// The right (foreground) child node.
        /// </summary>
        /// <userdoc>
        /// The foreground color mapping.
        /// </userdoc>
        [DataMember(30)]
        public IMaterialNode RightChild { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<MaterialNodeEntry> GetChildren(object context = null)
        {
            if (LeftChild != null)
            	yield return new MaterialNodeEntry(LeftChild, node => LeftChild = node);
            if (RightChild != null)
           	yield return new MaterialNodeEntry(RightChild, node => RightChild = node);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Binary operation";
        }
    }
}
