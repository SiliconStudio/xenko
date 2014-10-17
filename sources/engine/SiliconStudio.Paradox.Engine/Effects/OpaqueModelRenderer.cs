// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Paradox.Effects
{
    public class OpaqueModelRenderer : ModelRenderer
    {
        #region Constructor

        public OpaqueModelRenderer(IServiceRegistry services, string effectName)
            : base(services, effectName)
        {
        }

        #endregion

        #region Protected methods

        protected override void PrepareMeshesForRendering(RenderModel renderModel, Model model, ParameterCollection modelComponentParameters)
        {
            foreach (var mesh in model.Meshes)
            {
                if (!mesh.Material.Parameters.Get(MaterialParameters.UseTransparent))
                {
                    var effectMesh = new EffectMesh(null, mesh);
                    CreateEffect(effectMesh, modelComponentParameters);

                    // Register mesh for rendering
                    if (renderModel.InternalMeshes[MeshPassSlot] == null)
                    {
                        renderModel.InternalMeshes[MeshPassSlot] = new List<EffectMesh>();
                    }
                    renderModel.InternalMeshes[MeshPassSlot].Add(effectMesh);
                }
            }
        }

        protected override void UpdateMeshes(RenderContext context, ref FastList<EffectMesh> meshes)
        {
            base.UpdateMeshes(context, ref meshes);

            for (var i = 0; i < meshes.Count; ++i)
            {
                if (meshes[i].MeshData.Material.Parameters.Get(MaterialParameters.UseTransparent) || (context.ActiveLayers & meshes[i].MeshData.Layer) == 0)
                {
                    meshes.SwapRemoveAt(i--);
                }
            }

            //base.UpdateMeshes(context, ref meshes);
        }

        #endregion
    }
}
