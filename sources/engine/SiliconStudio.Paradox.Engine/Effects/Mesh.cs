// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Effects
{
    [DataConverter(AutoGenerate = true)]
    public class Mesh
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh()
        {
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class using a shallow copy constructor.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public Mesh(Mesh mesh)
        {
            Draw = mesh.Draw;
            
            // TODO: share parameter collection or copy parameters in a new one?
            Parameters = mesh.Parameters;
            
            Material = mesh.Material;
            NodeIndex = mesh.NodeIndex;
            Name = mesh.Name;
            BoundingBox = mesh.BoundingBox;
            Skinning = mesh.Skinning;
        }

        [DataMemberConvert]
        public MeshDraw Draw { get; set; }

        [DataMemberConvert]
        public Material Material { get; set; }
        
        [DataMemberConvert]
        public ParameterCollection Parameters { get; set; }
        
        /// <summary>
        /// Index of the transformation node in <see cref="Model"/>.
        /// </summary>
        [DataMemberConvert]
        public int NodeIndex;

        [DataMemberConvert]
        public string Name;

        /// <summary>
        /// Gets or sets the bounding box encompassing this <see cref="Mesh"/>.
        /// </summary>
        [DataMemberConvert]
        public BoundingBox BoundingBox;

        // TODO: Skinning could be shared between multiple Mesh inside a ModelView (multimaterial, etc...)
        [DataMemberConvert]
        public MeshSkinningDefinition Skinning;
    }
}