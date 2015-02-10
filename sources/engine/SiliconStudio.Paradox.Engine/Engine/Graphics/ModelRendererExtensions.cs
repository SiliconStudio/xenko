// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Engine.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Extensions filter for <see cref="ModelRenderer"/>
    /// </summary>
    public static class ModelRendererExtensions
    {
        /// <summary>
        /// Adds a default frustum culling for rendering only meshes that are only inside the frustum/
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <returns>ModelRenderer.</returns>
        public static ModelRenderer AddDefaultFrustumCulling(this ModelRenderer modelRenderer)
        {
            return modelRenderer.UpdateMeshes.Add(
                (context, meshes) =>
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
                        var boundingBox = renderMesh.Mesh.BoundingBox;
                        var center = boundingBox.Center;
                        var extent = boundingBox.Extent;

                        Vector3.TransformCoordinate(ref center, ref mat1, out center);

                        // Update world matrix into absolute form
                        unsafe
                        {
                            float* matrixData = &mat1.M11;
                            for (int j = 0; j < 16; ++j)
                            {
                                *matrixData = Math.Abs(*matrixData);
                                ++matrixData;
                            }
                        }

                        Vector3.TransformNormal(ref extent, ref mat1, out extent);

                        // Perform frustum culling
                        if (!Collision.FrustumContainsBox(ref frustum, ref center, ref extent))
                        {
                            meshes.SwapRemoveAt(i--);
                        }
                    }
                }
                );
        }
    }
}
