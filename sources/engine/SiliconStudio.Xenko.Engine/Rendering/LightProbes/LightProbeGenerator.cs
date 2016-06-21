// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.ComputeEffect.LambertianPrefiltering;
using SiliconStudio.Xenko.Rendering.Images.SphericalHarmonics;
using SiliconStudio.Xenko.Rendering.LightProbes;

namespace SiliconStudio.Xenko.Rendering.LightProbes
{
    public static class LightProbeGenerator
    {
        private const int LambertHamonicOrder = 3;

        public static List<LightProbeComponent> GenerateCoefficients(ISceneRendererContext context)
        {
            var lightProbeCamera = new CameraComponent
            {
                UseCustomProjectionMatrix = true,
                UseCustomViewMatrix = true,
                Slot = context.SceneSystem.GraphicsCompositor.Cameras.Count,
            };

            context.SceneSystem.GraphicsCompositor.Cameras.Add(new SceneCameraSlot(lightProbeCamera));

            // Replace graphics compositor (don't want post fx, etc...)
            var gameCompositor = context.SceneSystem.GraphicsCompositor.Game;
            context.SceneSystem.GraphicsCompositor.Game = new SceneCameraRenderer { Child = context.SceneSystem.GraphicsCompositor.SingleView, Camera = lightProbeCamera.Slot };

            // Setup projection matrix
            lightProbeCamera.ProjectionMatrix = Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(90.0f), 1.0f, lightProbeCamera.NearClipPlane, lightProbeCamera.FarClipPlane);

            // Create target cube texture
            var cubeTexture = Texture.NewCube(context.GraphicsDevice, 256, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource);

            // We can't render directly to the texture cube before feature level 10.1, so let's copy instead
            var renderTarget = Texture.New2D(context.GraphicsDevice, 256, 256, PixelFormat.R16G16B16A16_Float, TextureFlags.RenderTarget);
            var depthStencil = Texture.New2D(context.GraphicsDevice, 256, 256, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);

            var renderContext = RenderContext.GetShared(context.Services);

            // Prepare shader for SH prefiltering
            var lamberFiltering = new LambertianPrefilteringSHNoCompute(renderContext)
            {
                HarmonicOrder = LambertHamonicOrder,
                RadianceMap = cubeTexture,
            };
            var renderSHEffect = new SphericalHarmonicsRendererEffect();
            var renderDrawContext = new RenderDrawContext(context.Services, renderContext, context.GraphicsContext);

            var lightProbes = new List<LightProbeComponent>();

            using (renderDrawContext.PushRenderTargetsAndRestore())
            {
                // Render light probe
                context.GraphicsContext.CommandList.BeginProfile(Color.Red, "LightProbes");

                int lightProbeIndex = 0;
                foreach (var entity in context.SceneSystem.SceneInstance)
                {
                    var lightProbe = entity.Get<LightProbeComponent>();
                    if (lightProbe == null)
                        continue;

                    lightProbes.Add(lightProbe);

                    var lightProbePosition = lightProbe.Entity.Transform.WorldMatrix.TranslationVector;
                    context.GraphicsContext.ResourceGroupAllocator.Reset(context.GraphicsContext.CommandList);

                    context.GraphicsContext.CommandList.BeginProfile(Color.Red, $"LightProbes {lightProbeIndex}");
                    lightProbeIndex++;

                    for (int face = 0; face < 6; ++face)
                    {
                        // Place camera
                        switch ((CubeMapFace)face)
                        {
                            case CubeMapFace.PositiveX:
                                lightProbeCamera.ViewMatrix = Matrix.LookAtRH(lightProbePosition, lightProbePosition + Vector3.UnitX, Vector3.UnitY);
                                break;
                            case CubeMapFace.NegativeX:
                                lightProbeCamera.ViewMatrix = Matrix.LookAtRH(lightProbePosition, lightProbePosition - Vector3.UnitX, Vector3.UnitY);
                                break;
                            case CubeMapFace.PositiveY:
                                lightProbeCamera.ViewMatrix = Matrix.LookAtRH(lightProbePosition, lightProbePosition + Vector3.UnitY, Vector3.UnitZ);
                                break;
                            case CubeMapFace.NegativeY:
                                lightProbeCamera.ViewMatrix = Matrix.LookAtRH(lightProbePosition, lightProbePosition - Vector3.UnitY, -Vector3.UnitZ);
                                break;
                            case CubeMapFace.PositiveZ:
                                lightProbeCamera.ViewMatrix = Matrix.LookAtRH(lightProbePosition, lightProbePosition - Vector3.UnitZ, Vector3.UnitY);
                                break;
                            case CubeMapFace.NegativeZ:
                                lightProbeCamera.ViewMatrix = Matrix.LookAtRH(lightProbePosition, lightProbePosition + Vector3.UnitZ, Vector3.UnitY);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        context.GraphicsContext.CommandList.BeginProfile(Color.Red, $"Face {(CubeMapFace)face}");

                        // Draw
                        context.GraphicsContext.CommandList.SetRenderTargetAndViewport(depthStencil, renderTarget);
                        context.GameSystems.Draw(context.DrawTime);

                        // Copy to texture cube
                        context.GraphicsContext.CommandList.CopyRegion(renderTarget, 0, null, cubeTexture, face);

                        context.GraphicsContext.CommandList.EndProfile();
                    }

                    context.GraphicsContext.CommandList.BeginProfile(Color.Red, "Prefilter SphericalHarmonics");

                    // Compute SH coefficients
                    lamberFiltering.Draw(renderDrawContext);

                    var coefficients = lamberFiltering.PrefilteredLambertianSH.Coefficients;
                    lightProbe.Coefficients = new FastList<Color3>();
                    for (int i = 0; i < coefficients.Length; i++)
                    {
                        lightProbe.Coefficients.Add(coefficients[i] * SphericalHarmonics.BaseCoefficients[i]);
                    }

                    using (var outputCubemap = Texture.NewCube(context.GraphicsDevice, 256, 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource))
                    {
                        renderSHEffect.InputSH = lamberFiltering.PrefilteredLambertianSH;
                        renderSHEffect.SetOutput(outputCubemap);
                        renderSHEffect.Draw(renderDrawContext);

                        // Save cubemaps to HDD for debugging purpose
                        //using (var file = File.Create($"test{lightProbeIndex}.dds"))
                        //    cubeTexture.Save(context.GraphicsContext.CommandList, file, ImageFileType.Dds);
                        //
                        //using (var file = File.Create($"test{lightProbeIndex}-filtered.dds"))
                        //    outputCubemap.Save(context.GraphicsContext.CommandList, file, ImageFileType.Dds);
                    }

                    context.GraphicsContext.CommandList.EndProfile(); // Prefilter SphericalHarmonics

                    context.GraphicsContext.CommandList.EndProfile(); // Face XXX

                    // Debug render
                }

                context.GraphicsContext.CommandList.EndProfile(); // LightProbes
            }

            context.SceneSystem.GraphicsCompositor.Game = gameCompositor;
            context.SceneSystem.GraphicsCompositor.Cameras.RemoveAt(context.SceneSystem.GraphicsCompositor.Cameras.Count - 1);

            return lightProbes;
        }

        public static unsafe LightProbeRuntimeData GenerateRuntimeData(SceneInstance sceneInstance)
        {
            // Find lightprobes
            var lightProbes = new FastList<LightProbeComponent>();
            foreach (var entity in sceneInstance)
            {
                var lightProbe = entity.Get<LightProbeComponent>();
                if (lightProbe != null)
                {
                    entity.Transform.UpdateWorldMatrix();
                    lightProbes.Add(lightProbe);
                }
            }

            // TODO: Better check: coplanar, etc... (maybe the check inside BowyerWatsonTetrahedralization might be enough -- tetrahedron won't be in positive order)
            if (lightProbes.Count < 4)
                throw new InvalidOperationException("Can't generate lightprobes if less than 4 of them exists.");

            var lightProbePositions = new FastList<Vector3>();
            var lightProbeCoefficients = new Color3[lightProbes.Count * LambertHamonicOrder * LambertHamonicOrder];
            fixed (Color3* destColors = lightProbeCoefficients)
            {
                for (var lightProbeIndex = 0; lightProbeIndex < lightProbes.Count; lightProbeIndex++)
                {
                    var lightProbe = lightProbes[lightProbeIndex];

                    // Copy light position
                    lightProbePositions.Add(lightProbe.Entity.Transform.WorldMatrix.TranslationVector);

                    // Copy coefficients
                    if (lightProbe.Coefficients != null)
                    {
                        var lightProbeCoefStart = lightProbeIndex * LambertHamonicOrder * LambertHamonicOrder;
                        for (var index = 0; index < LambertHamonicOrder * LambertHamonicOrder; index++)
                        {
                            destColors[lightProbeCoefStart + index] = index < lightProbe.Coefficients.Count ? lightProbe.Coefficients[index] : new Color3();
                        }
                    }
                }
            }

            // Generate light probe structure
            var tetra = new BowyerWatsonTetrahedralization();
            var tetraResult = tetra.Compute(lightProbePositions);

            var matrices = new Vector4[tetraResult.Tetrahedra.Count * 3];
            var probeIndices = new Int4[tetraResult.Tetrahedra.Count];

            // Prepare data for GPU: matrices and indices
            for (int i = 0; i < tetraResult.Tetrahedra.Count; ++i)
            {
                var tetrahedron = tetraResult.Tetrahedra[i];
                var tetrahedronMatrix = Matrix.Identity;

                // Compute the tetrahedron matrix
                // https://en.wikipedia.org/wiki/Barycentric_coordinate_system#Barycentric_coordinates_on_tetrahedra
                var vertex3 = tetraResult.Vertices[tetrahedron.Vertices[3]];
                *((Vector3*)&tetrahedronMatrix.M11) = tetraResult.Vertices[tetrahedron.Vertices[0]] - vertex3;
                *((Vector3*)&tetrahedronMatrix.M12) = tetraResult.Vertices[tetrahedron.Vertices[1]] - vertex3;
                *((Vector3*)&tetrahedronMatrix.M13) = tetraResult.Vertices[tetrahedron.Vertices[2]] - vertex3;
                tetrahedronMatrix.Invert(); // TODO: Optimize 3x3 invert

                tetrahedronMatrix.Transpose();

                // Store position of last vertex in last row
                tetrahedronMatrix.M41 = vertex3.X;
                tetrahedronMatrix.M42 = vertex3.Y;
                tetrahedronMatrix.M43 = vertex3.Z;

                matrices[i * 3 + 0] = tetrahedronMatrix.Column1;
                matrices[i * 3 + 1] = tetrahedronMatrix.Column2;
                matrices[i * 3 + 2] = tetrahedronMatrix.Column3;

                probeIndices[i] = *(Int4*)tetrahedron.Vertices;
            }

            var result = new LightProbeRuntimeData
            {
                Vertices = tetraResult.Vertices,
                UserVertexCount = tetraResult.UserVertexCount,
                Tetrahedra = tetraResult.Tetrahedra,
                Faces = tetraResult.Faces,

                Coefficients = lightProbeCoefficients,
                Matrices = matrices,
                LightProbeIndices = probeIndices,
            };

            return result;
        }
    }

    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<LightProbeRuntimeData>))]
    public class LightProbeRuntimeData
    {
        public Vector3[] Vertices;
        public int UserVertexCount;
        public FastList<BowyerWatsonTetrahedralization.Tetrahedron> Tetrahedra;
        public FastList<BowyerWatsonTetrahedralization.Face> Faces;

        // Data to upload to GPU
        public Color3[] Coefficients;
        public Vector4[] Matrices;
        public Int4[] LightProbeIndices;
    }
}