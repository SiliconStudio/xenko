// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Extensions filter for <see cref="ModelComponentRenderer"/>
    /// </summary>
    public static class ModelRendererExtensions
    {
        // TODO: Find a way to replug this

        /// <summary>
        /// Adds a default frustum culling for rendering only meshes that are only inside the frustum/
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <returns>ModelRenderer.</returns>
        //public static ModelComponentRenderer AddDefaultFrustumCulling(this ModelComponentRenderer modelRenderer)
        //{
        //    modelRenderer.UpdateMeshes = FrustumCulling;
        //    return modelRenderer;
        //}

        private static void FrustumCulling(RenderContext context, FastList<RenderMesh> meshes)
        {
            Matrix viewProjection, mat1, mat2;

            // Compute view * projection
            context.Parameters.Get(TransformationKeys.View, out mat1);
            context.Parameters.Get(TransformationKeys.Projection, out mat2);
            Matrix.Multiply(ref mat1, ref mat2, out viewProjection);

            var frustum = new BoundingFrustum(ref viewProjection);

            for (var i = 0; i < meshes.Count; ++i)
            {
                var renderMesh = meshes[i];

                // Fast AABB transform: http://zeuxcg.org/2010/10/17/aabb-from-obb-with-component-wise-abs/
                // Get world matrix
                renderMesh.Mesh.Parameters.Get(TransformationKeys.World, out mat1);

                // Compute transformed AABB (by world)
                var boundingBoxExt = new BoundingBoxExt(renderMesh.Mesh.BoundingBox);
                boundingBoxExt.Transform(mat1);

                // Perform frustum culling
                if (!frustum.Contains(ref boundingBoxExt))
                {
                    meshes.SwapRemoveAt(i--);
                }
            }
        }
    }
}
