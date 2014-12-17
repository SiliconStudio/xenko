// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialReferenceNode>))]
    [DataContract("MaterialReferenceNode")]
    public sealed class MaterialReferenceNode : MaterialNodeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialReferenceNode"/> class.
        /// </summary>
        public MaterialReferenceNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialReferenceNode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MaterialReferenceNode(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name this instance is linked to.
        /// </summary>
        /// <value>The name.</value>
        /// <userdoc>
        /// The name of the referenced node.
        /// </userdoc>
        [DataMember(10)]
        public string Name { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<MaterialNodeEntry> GetChildren(object context = null)
        {
            var materialContext = context as MaterialContext;
            if (materialContext != null && Name != null)
            {
                IMaterialNode materialNode;
                if (materialContext.Material.Nodes.TryGetValue(Name, out materialNode))
                {
                    if (materialNode != null && materialNode != this)
                        yield return new MaterialNodeEntry(materialNode, node => { });
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Reference";
        }
    }
}
