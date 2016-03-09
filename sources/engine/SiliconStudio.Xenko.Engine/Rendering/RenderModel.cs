// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Contains information related to the <see cref="Rendering.Model"/> so that the <see cref="RenderMesh"/> can access it.
    /// </summary>
    public class RenderModel
    {
        public readonly ModelComponent ModelComponent;
        public Model Model;
        public RenderMesh[] Meshes;

        public RenderModel(ModelComponent modelComponent)
        {
            ModelComponent = modelComponent;
        }
    }
}