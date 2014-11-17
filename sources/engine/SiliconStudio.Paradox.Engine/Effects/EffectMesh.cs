// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    public class EffectMesh
    {
        internal VertexArrayObject VertexArrayObject;
        // Kept as internal, as it's a dependency we probably don't want to show.
        // It should be stored in Tags, but cached here for better performance.
        // Could be an "object" if we want to reduce dependencies.
        internal IModelInstance ModelInstance;

        /// <summary>
        /// Gets the attached properties to this component.
        /// </summary>
        public PropertyContainer Tags;

        internal EffectParameterUpdaterDefinition UpdaterDefinition { get; set; }

        public MeshDraw RenderData { get; set; }

        public Effect Effect { get; set; }

        public Mesh Mesh { get; set; }

        public ParameterCollection Parameters { get; private set; }

        public bool Enabled { get; set; }

        public bool IsTransparent { get; set; }

        public int ConfigurationIndex { get; set; }

        /// <summary>
        /// The start action.
        /// </summary>
        public Action<RenderContext, EffectMesh> Render;

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectMesh" /> class.
        /// </summary>
        /// <param name="effect">The effect.</param>
        /// <param name="mesh">The mesh data.</param>
        public EffectMesh(Effect effect, Mesh mesh = null)
        {
            Enabled = true;

            if (mesh == null)
                mesh = new Mesh();

            Parameters = mesh.Parameters ?? new ParameterCollection("Mesh Parameters");

            Mesh = mesh;
            Effect = effect;
            Tags = new PropertyContainer(this);

            IsTransparent = Mesh.Material != null && Mesh.Material.Parameters.Get(MaterialParameters.UseTransparent);

            ConfigurationIndex = -1;
        }
    }
}
