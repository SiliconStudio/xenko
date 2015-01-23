// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Extensions filter for <see cref="ModelRenderer"/>
    /// </summary>
    public static class ModelRendererExtensions
    {
        // TODO: Add support for OR combination of filters

        /// <summary>
        /// Adds a transparent filter for rendering meshes which are transparent.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <returns>ModelRenderer.</returns>
        public static ModelRenderer AddTransparentFilter(this ModelRenderer modelRenderer)
        {
            modelRenderer.AcceptPrepareMeshForRendering.Add((model, mesh) => IsTransparent(mesh));
            modelRenderer.AcceptRenderMesh.Add((context, renderMesh) => IsTransparent(renderMesh.Mesh));
            modelRenderer.AppendDebugName("Transparent");
            return modelRenderer;
        }

        /// <summary>
        /// Adds an opaque filter for rendering meshes which are opaque.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <returns>ModelRenderer.</returns>
        public static ModelRenderer AddOpaqueFilter(this ModelRenderer modelRenderer)
        {
            modelRenderer.AcceptPrepareMeshForRendering.Add((model, mesh) => !IsTransparent(mesh));
            modelRenderer.AcceptRenderMesh.Add((context, renderMesh) => !IsTransparent(renderMesh.Mesh));
            modelRenderer.AppendDebugName("Opaque");
            return modelRenderer;
        }

        private static bool IsTransparent(Mesh mesh)
        {
            return mesh.Material.Parameters.Get(MaterialParameters.UseTransparent);
        }

        /// <summary>
        /// Adds a layer filter for rendering meshes only on the specified layer.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <param name="activelayers">The activelayers.</param>
        /// <returns>ModelRenderer.</returns>
        public static ModelRenderer AddLayerFilter(this ModelRenderer modelRenderer, RenderLayers activelayers)
        {
            modelRenderer.AcceptRenderMesh.Add((context, renderMesh) => (renderMesh.Parameters.Get(RenderingParameters.RenderLayer) & activelayers) != RenderLayers.RenderLayerNone);
            modelRenderer.AppendDebugName("Layer " + activelayers);
            return modelRenderer;
        }

        /// <summary>
        /// Adds a layer filter for rendering meshes only on the context active layers.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <returns>ModelRenderer.</returns>
        public static ModelRenderer AddContextActiveLayerFilter(this ModelRenderer modelRenderer)
        {
            modelRenderer.AcceptRenderMesh.Add((context, renderMesh) => (context.Parameters.Get(RenderingParameters.ActiveRenderLayer) & renderMesh.Parameters.Get(RenderingParameters.RenderLayer)) != RenderLayers.RenderLayerNone);
            modelRenderer.AppendDebugName("Active Layer");
            return modelRenderer;
        }

        /// <summary>
        /// Adds a shadow caster filter for rendering only meshes that can cast shadows.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <returns>ModelRenderer.</returns>
        public static ModelRenderer AddShadowCasterFilter(this ModelRenderer modelRenderer)
        {
            modelRenderer.AcceptPrepareMeshForRendering.Add((model, mesh) => mesh.Parameters.Get(LightingKeys.CastShadows));
            modelRenderer.AcceptRenderMesh.Add((context, renderMesh) => renderMesh.Parameters.Get(LightingKeys.CastShadows));
            modelRenderer.AppendDebugName("ShadowMapCaster");
            return modelRenderer;
        }

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
                    modelRenderer.Pass.Parameters.Get(TransformationKeys.View, out mat1);
                    modelRenderer.Pass.Parameters.Get(TransformationKeys.Projection, out mat2);
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
