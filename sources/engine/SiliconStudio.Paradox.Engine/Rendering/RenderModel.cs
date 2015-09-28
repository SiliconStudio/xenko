// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering
{
    public class RenderModel
    {
        public RenderModel(ModelComponent modelComponent)
        {
            RenderMeshesPerEffectSlot = new FastListStruct<FastListStruct<RenderMesh>>(4);
            ModelComponent = modelComponent;
        }

        public readonly ModelComponent ModelComponent;

        internal FastListStruct<FastListStruct<RenderMesh>> RenderMeshesPerEffectSlot;

        private Model previousModel;

        public bool Update()
        {
            if (!ModelComponent.Enabled || ModelComponent.ModelViewHierarchy == null || ModelComponent.Model == null)
            {
                return false;
            }

            var newModel = ModelComponent.Model;
            if (previousModel != newModel)
            {
                // When changing the model, we need to regenerate the render meshes
                for(int i = 0; i < RenderMeshesPerEffectSlot.Count; i++)
                {
                    // TODO: We should dispose render mehses here, but need to check exactly how
                    // Changing a struct so make a copy first (TODO: Replace with ref locals when released)
                    var renderMeshes = RenderMeshesPerEffectSlot[i];
                    renderMeshes.Clear();
                    RenderMeshesPerEffectSlot[i] = renderMeshes;
                }

                previousModel = newModel;
            }

            return true;
        }

        public Material GetMaterial(int materialIndex)
        {
            // TBD, but for now, -1 means null material
            if (materialIndex == -1)
                return null;

            // Try to get material first from model instance, then model
            return ModelComponent.Materials.GetItemOrNull(materialIndex)
                ?? GetMaterialHelper(ModelComponent.Model.Materials, materialIndex);
        }

        public MaterialInstance GetMaterialInstance(int materialIndex)
        {
            return ModelComponent.Model.Materials.GetItemOrNull(materialIndex);
        }

        private static Material GetMaterialHelper(List<MaterialInstance> materials, int index)
        {
            if (materials != null && index < materials.Count)
            {
                return materials[index].Material;
            }

            return null;
        }
    }
}