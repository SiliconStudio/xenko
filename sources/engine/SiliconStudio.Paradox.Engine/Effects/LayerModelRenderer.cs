// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Paradox.Effects
{
    public class LayerModelRenderer : ModelRenderer
    {
        private RenderLayers activeLayers;
        public LayerModelRenderer(IServiceRegistry services, string effectName, RenderLayers layers)
            : base(services, effectName)
        {
            activeLayers = layers;
        }

        protected override void UpdateMeshes(RenderContext context, ref FastList<EffectMesh> meshes)
        {
            for (var i = 0; i < meshes.Count; ++i)
            {
                if ((meshes[i].MeshData.Layer & activeLayers) == 0)
                    meshes.SwapRemoveAt(i--);
            }

            base.UpdateMeshes(context, ref meshes);
        }
    }
}