// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
