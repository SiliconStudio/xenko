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
        [DataMemberIgnore]
        private bool castShadows;
        
        [DataMemberIgnore]
        private bool receiveShadows;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh()
        {
            Parameters = new ParameterCollection();
            Layer = RenderLayers.RenderLayerAll;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class using a shallow copy constructor.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public Mesh(Mesh mesh)
        {
            Draw = mesh.Draw;
            Parameters = mesh.Parameters ?? new ParameterCollection();
            Material = mesh.Material;
            NodeIndex = mesh.NodeIndex;
            Name = mesh.Name;
            BoundingBox = mesh.BoundingBox;
            Skinning = mesh.Skinning;
            Layer = mesh.Layer;
            CastShadows = mesh.CastShadows;
            ReceiveShadows = mesh.ReceiveShadows;
            if (mesh.Lighting != null)
            {
                Lighting = new LightingConfigurationsSet();
                Lighting.Configs = mesh.Lighting.Configs.ToArray();
            }
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

        /// <summary>
        /// The layer the model belongs to.
        /// </summary>
        /// <value>The layer mask.</value>
        [DataMemberConvert]
        public RenderLayers Layer { get; set; }

        /// <summary>
        /// The mesh casts shadow.
        /// </summary>
        [DataMemberConvert]
        public bool CastShadows
        {
            get
            {
                return castShadows;
            }
            set
            {
                if (value != castShadows)
                {
                    castShadows = value;
                    if (Parameters == null)
                        Parameters = new ParameterCollection();
                    Parameters.Set(LightingKeys.CastShadows, castShadows);
                }
            }
        }
        
        /// <summary>
        /// The mesh receives shadow.
        /// </summary>
        [DataMemberConvert]
        public bool ReceiveShadows
        {
            get
            {
                return receiveShadows;
            }
            set
            {
                if (value != receiveShadows)
                {
                    receiveShadows = value;
                    if (Parameters == null)
                        Parameters = new ParameterCollection();
                    Parameters.Set(LightingKeys.ReceiveShadows, receiveShadows);
                }
            }
        }

        /// <summary>
        /// The list of available lighting configurations. Should be sorted based on the total number of lights.
        /// </summary>
        [DataMemberConvert]
        public LightingConfigurationsSet Lighting { get; set; }
    }
}