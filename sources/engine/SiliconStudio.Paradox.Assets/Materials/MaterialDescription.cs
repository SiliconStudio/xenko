// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Description of a material.
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<MaterialDescription>))]
    [DataContract("Material")]
    public sealed class MaterialDescription
    {
        /// <summary>
        /// The tree describing the flow of instructions leading to this material.
        /// </summary>
        /// <userdoc>
        /// All the color mapping nodes of the materials. They are map descriptions (texture or values) and operations on them.
        /// </userdoc>
        [DataMember(10)]
        public Dictionary<string, IMaterialNode> Nodes { get; set; }

        /// <summary>
        /// The tree used in this model.
        /// </summary>
        /// <userdoc>
        /// The final output of the material. Each item references a node and put it behind the chosen ParameterKey.
        /// </userdoc>
        [DataMember(30)]
        public Dictionary<ParameterKey<ShaderMixinSource>, string> ColorNodes { get; set; }

        /// <summary>
        /// The parameters of this model.
        /// </summary>
        /// <userdoc>
        /// The parameters of the material. Any parameter can be set here. This is the lowest priority collection so it can be overridden by the model and mesh parameters.
        /// </userdoc>
        [DataMember(50)]
        public ParameterCollectionData Parameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDescription"/> class.
        /// </summary>
        public MaterialDescription()
        {
            //MaterialTrees = new Dictionary<string, MaterialTree>();
            Nodes = new Dictionary<string, IMaterialNode>();
            ColorNodes = new Dictionary<ParameterKey<ShaderMixinSource>, string>();
            Parameters = new ParameterCollectionData();
        }

        /// <summary>
        /// Inserts a new tree in the material
        /// </summary>
        /// <param name="referenceName">The name of the reference.</param>
        /// <param name="node">The material onde.</param>
        public void AddNode(string referenceName, IMaterialNode node)
        {
            if (Nodes.ContainsKey(referenceName))
                throw new Exception("A reference with name " + referenceName + " already exists");
            Nodes.Add(referenceName, node);
        }

        /// <summary>
        /// Adds a tree in the model.
        /// </summary>
        /// <param name="key">The key of the slot in the model.</param>
        /// <param name="referenceName">The name of the tree.</param>
        /// <param name="node">The node to add.</param>
        public void AddColorNode(ParameterKey<ShaderMixinSource> key, string referenceName, IMaterialNode node)
        {
            if (ColorNodes.ContainsKey(key))
                ColorNodes[key] = referenceName;
            else
                ColorNodes.Add(key, referenceName);

            AddNode(referenceName, node);
        }

        /// <summary>
        /// Returns the material node corresponding to the brdf slot.
        /// </summary>
        /// <param name="key">The key of the brdf slot.</param>
        /// <returns>The material node.</returns>
        public IMaterialNode GetMaterialNode(ParameterKey<ShaderMixinSource> key)
        {
            string treeName;
            if (ColorNodes.TryGetValue(key, out treeName))
            {
                IMaterialNode materialTree;
                if (Nodes.TryGetValue(treeName, out materialTree))
                    return materialTree;
            }
            return null;
        }

        /// <summary>
        /// Returns the material node corresponding to the reference
        /// </summary>
        /// <param name="referenceName">The name of the reference.</param>
        /// <returns>The material node.</returns>
        public IMaterialNode FindNode(string referenceName)
        {
            IMaterialNode materialTree;
            if (referenceName != null && Nodes.TryGetValue(referenceName, out materialTree))
                return materialTree;
            return null;
        }

        /// <summary>
        /// Create a reference to a node.
        /// </summary>
        /// <param name="node">The material node.</param>
        /// <param name="referenceName">The name of the reference.</param>
        public void MakeReference(IMaterialNode node, string referenceName)
        {
            IMaterialNode prevNode;
            if (Nodes.TryGetValue(referenceName, out prevNode))
            {
                if (!ReferenceEquals(node, prevNode))
                    throw new Exception("Unable to create a reference with the name " + referenceName + " because there is already a reference with that name.");
            }
            else
            {
                var matReference = new MaterialReferenceNode(referenceName);
                foreach (var tree in Nodes.Select(x => x.Value))
                {
                    //var replacer = new MaterialNodeReplacer.
                }
                Nodes.Add(referenceName, node);
            }
        }

        /// <summary>
        /// Set the value of a compilation parameter. Creates a new entry if necessary.
        /// </summary>
        /// <param name="parameterKey">The ParameterKey.</param>
        /// <param name="value">The value.</param>
        public void SetParameter(ParameterKey parameterKey, object value)
        {
            Parameters.Set(parameterKey, value);
        }

        /// <summary>
        /// Get the value of a compilation parameter. Creates a new entry if necessary.
        /// </summary>
        /// <param name="parameterKey">The parameter key.</param>
        /// <returns>The value of the parameter.</returns>
        public T GetParameter<T>(ParameterKey<T> parameterKey)
        {
            return (T)Parameters[parameterKey];
        }

        /// <summary>
        /// Get all the Compilation parameters for this model.
        /// </summary>
        /// <returns>A collection of all the parameters.</returns>
        public ParameterCollectionData GetParameters()
        {
            return Parameters;
        }

        /// <inheritdoc/>
        public MaterialDescription Clone()
        {
            // TODO: use more efficient cloning method using binary serialization
            return (MaterialDescription)AssetCloner.Clone(this);
        }
    }
}
