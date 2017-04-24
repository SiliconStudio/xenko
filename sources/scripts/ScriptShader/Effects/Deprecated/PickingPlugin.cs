// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    public class PickingPlugin : RenderPassPlugin
    {
        private UpdatePassesDelegate updatePassesAction;

        public static PropertyKey<Entity> AssociatedEntity = new PropertyKey<Entity>("AssociatedEntity", typeof(PickingPlugin));

        public static readonly ParameterKey<Vector2> PickingScreenPosition = ParameterKeys.Value(Vector2.Zero);
        public static readonly ParameterKey<int> PickingFrameIndex = ParameterKeys.Value(0);
        public static readonly ParameterKey<int> PickingMeshIndex = ParameterKeys.Value(0);
        public static readonly ParameterKey<Matrix> PickingMatrix = ParameterKeys.Value(Matrix.Identity);

        private HashSet<Request> requestResults = new HashSet<Request>();
        private List<Request> pendingRequests = new List<Request>();
        private int currentPickingFrameIndex;

        /// <summary>
        /// Gets or sets the main plugin this instance is attached to.
        /// </summary>
        /// <value>
        /// The main plugin.
        /// </value>
        public MainPlugin MainPlugin { get; set; }

        public int PickingDistance { get; set; }

        public override void Load()
        {
            base.Load();

            if (OfflineCompilation)
                return;

            var renderTargets = new RenderTarget[2];
            DepthStencilBuffer depthStencilBuffer = null;
            Texture2D depthStencilTexture = null;

            Parameters.AddSources(MainPlugin.ViewParameters);

            Parameters.RegisterParameter(EffectPlugin.BlendStateKey);

            var filteredPasses = new FastList<RenderPass>();

            RenderPass.UpdatePasses += updatePassesAction = (RenderPass currentRenderPass, ref FastList<RenderPass> currentPasses) =>
                {
                    var originalPasses = currentPasses;
                    filteredPasses.Clear();
                    currentPasses = filteredPasses;

                    Parameters.Set(PickingFrameIndex, ++currentPickingFrameIndex);
                    Request[] requests;

                    lock (pendingRequests)
                    {
                        // No picking request or no mesh to pick?
                        if (pendingRequests.Count == 0)
                            return;

                        requests = pendingRequests.ToArray();
                        pendingRequests.Clear();
                    }

                    foreach (var request in requests)
                    {
                        requestResults.Add(request);
                    }

                    if (originalPasses == null)
                        return;

                    // Count mesh passes
                    int meshIndex = 0;
                    foreach (var pass in originalPasses)
                    {
                        meshIndex += pass.Passes.Count;
                    }

                    // No mesh to pick?
                    if (meshIndex == 0)
                        return;

                    // Copy mesh passes and assign indices
                    var meshPasses = new EffectMesh[meshIndex];
                    meshIndex = 0;
                    foreach (var pass in RenderPass.Passes)
                    {
                        throw new NotImplementedException();
                        //foreach (var effectMeshPass in pass.Meshes)
                        //{
                        //    meshPasses[meshIndex] = (EffectMesh)effectMeshPass;
                        //    // Prefix increment so that 0 means no rendering.
                        //    effectMeshPass.Parameters.Set(PickingMeshIndex, ++meshIndex);
                        //}
                    }

                    // For now, it generates one rendering per picking.
                    // It would be quite easy to optimize it by make Picking shader works on multiple picking points at a time.
                    foreach (var request in requests)
                    {
                        var pickingRenderPass = new RenderPass("Picking");

                        pickingRenderPass.StartPass.AddFirst = (threadContext) =>
                            {
                                threadContext.GraphicsDevice.Clear(renderTargets[0], Color.Black);
                                threadContext.GraphicsDevice.Clear(renderTargets[1], Color.Black);
                                threadContext.Parameters.Set(PickingScreenPosition, request.Location);
                                threadContext.GraphicsDevice.SetViewport(new Viewport(0, 0, renderTargets[0].Description.Width, renderTargets[0].Description.Height));

                                threadContext.GraphicsDevice.Clear(depthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
                                threadContext.GraphicsDevice.SetRenderTargets(depthStencilBuffer, renderTargets);
                            };
                        pickingRenderPass.EndPass.AddLast = (threadContext) =>
                            {
                                threadContext.Parameters.Reset(PickingScreenPosition);
                                threadContext.GraphicsDevice.Copy(renderTargets[0].Texture, request.ResultTextures[0]);
                                threadContext.GraphicsDevice.Copy(renderTargets[1].Texture, request.ResultTextures[1]);
                            };
                        //pickingRenderPass.PassesInternal = originalPasses;
                        throw new NotImplementedException();

                        request.MeshPasses = meshPasses;

                        currentPasses.Add(pickingRenderPass);

                        request.HasResults = true;

                        // Wait 2 frames before pulling the results.
                        request.FrameCounter = 2;
                    }
                };

            RenderSystem.GlobalPass.EndPass.AddLast = CheckPickingResults;

            var backBuffer = GraphicsDevice.BackBuffer; 

            int pickingArea = 1 + PickingDistance * 2;
            renderTargets[0] = Texture.New2D(GraphicsDevice, pickingArea, pickingArea, PixelFormat.R32_UInt, TextureFlags.ShaderResource | TextureFlags.RenderTarget).ToRenderTarget().KeepAliveBy(ActiveObjects);
            renderTargets[1] = Texture.New2D(GraphicsDevice, pickingArea, pickingArea, PixelFormat.R32G32B32A32_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget).ToRenderTarget().KeepAliveBy(ActiveObjects);

            depthStencilTexture = Texture.New2D(GraphicsDevice, pickingArea, pickingArea, PixelFormat.D32_Float, TextureFlags.ShaderResource | TextureFlags.DepthStencil).KeepAliveBy(ActiveObjects);
            depthStencilBuffer = depthStencilTexture.ToDepthStencilBuffer(false);

            Parameters.AddDynamic(PickingMatrix, ParameterDynamicValue.New(PickingScreenPosition, (ref Vector2 pickingPosition, ref Matrix picking) =>
                {
                    // Move center to picked position, and zoom (it is supposed to stay per-pixel according to render target size)
                    picking = Matrix.Translation(1.0f - (pickingPosition.X) / backBuffer.Width * 2.0f, -1.0f + (pickingPosition.Y) / backBuffer.Height * 2.0f, 0.0f)
                        * Matrix.Scaling((float)backBuffer.Width / (float)pickingArea, (float)backBuffer.Height / (float)pickingArea, 1.0f);
                }));
        }

        public override void Unload()
        {
            if (!OfflineCompilation)
            {
                RenderPass.UpdatePasses = (UpdatePassesDelegate)Delegate.Remove(RenderPass.UpdatePasses, updatePassesAction);
                Parameters.RemoveSource(MainPlugin.ViewParameters);
                RenderSystem.GlobalPass.EndPass -= CheckPickingResults;
            }

            base.Unload();
        }

        private unsafe void CheckPickingResults(ThreadContext threadContext)
        {
            if (requestResults.Count == 0)
                return;

            foreach (var request in requestResults.ToArray())
            {
                if (request.HasResults)
                {
                    if (--request.FrameCounter > 0)
                        continue;

                    // TODO rewrite picking with get/setdata

                    //var mapResultMeshId = threadContext.GraphicsDevice.Map(request.ResultTextures[0], 0, MapMode.Read);
                    //var mapResultPosition = threadContext.GraphicsDevice.Map(request.ResultTextures[1], 0, MapMode.Read);

                    //var meshIds = (byte*)mapResultMeshId.DataPointer;
                    //var positions = (byte*)mapResultPosition.DataPointer;

                    //var pickingArea = 1 + 2 * PickingDistance;
                    //float bestPickingDistance = float.PositiveInfinity;
                    //int meshId = 0;
                    //var position = Vector3.Zero;
                    //for (int y = 0; y < pickingArea; ++y)
                    //{
                    //    for (int x = 0; x < pickingArea; ++x)
                    //    {
                    //        var currentMeshId = *(int*)&meshIds[y * mapResultMeshId.RowPitch + x * sizeof(int)];
                    //        var pickingDistance = new Vector2(x - PickingDistance, y - PickingDistance).LengthSquared();
                    //        if (currentMeshId != 0 && pickingDistance < bestPickingDistance)
                    //        {
                    //            bestPickingDistance = pickingDistance;
                    //            meshId = currentMeshId;
                    //            position = *(Vector3*)&positions[y * mapResultPosition.RowPitch + x * sizeof(Vector4)];
                    //        }
                    //    }
                    //}

                    //threadContext.GraphicsDevice.Unmap(request.ResultTextures[0], 0);
                    //threadContext.GraphicsDevice.Unmap(request.ResultTextures[1], 0);

                    //Console.WriteLine("Picking: {0} new R32G32B32_Float({1}f,{2}f,{3}f)", meshId, position.X, position.Y, position.Z);

                    //var stream = new FileStream("picking.txt", FileMode.Append);
                    //var streamWriter = new StreamWriter(stream);
                    //streamWriter.WriteLine("new R32G32B32_Float({0}f,{1}f,{2}f)", position.X, position.Y, position.Z);
                    //streamWriter.Flush();
                    //stream.Close();

                    //request.PickedMesh = (meshId != 0) ? request.MeshPasses[meshId - 1].EffectMesh : null;
                    //request.PickedPosition = request.PickedMesh != null ? position : new Vector3(float.NaN, float.NaN, float.NaN);
                }
                else
                {
                    request.PickedMesh = null;
                    request.PickedPosition = new Vector3(float.NaN, float.NaN, float.NaN);
                }

                request.MeshPasses = null;

                request.ResultTextures[0].Release();
                request.ResultTextures[1].Release();

                request.TaskCompletionSource.SetResult(true);

                requestResults.Remove(request);
            }
        }

        public async Task<Result> Pick(Vector2 location)
        {
            var request = new Request();
            request.Location = location;

            // Create staging textures
            int pickingArea = 1 + PickingDistance * 2;
            request.ResultTextures = new Texture2D[2];
            request.ResultTextures[0] = Texture.New2D(GraphicsDevice, pickingArea, pickingArea, PixelFormat.R32_UInt, TextureFlags.None, usage: GraphicsResourceUsage.Staging);
            request.ResultTextures[0].Name = "PickingTextureStaging";

            request.ResultTextures[1] = Texture.New2D(GraphicsDevice, pickingArea, pickingArea, PixelFormat.R32G32B32A32_Float, TextureFlags.None, usage: GraphicsResourceUsage.Staging);
            request.ResultTextures[1].Name = "PickingTextureStaging";

            request.ResultTextures[0].AddReference();
            request.ResultTextures[1].AddReference();

            lock (pendingRequests)
            {
                pendingRequests.Add(request);
            }

            // Wait for results
            await request.TaskCompletionSource.Task;

            return new Result { EffectMesh = request.PickedMesh, Position = request.PickedPosition };
        }

        private class Request
        {
            public int FrameCounter;
            public bool HasResults;
            public Vector2 Location;
            public Texture2D[] ResultTextures;
            public TaskCompletionSource<bool> TaskCompletionSource = new TaskCompletionSource<bool>();
            public EffectMesh[] MeshPasses;
            public EffectMesh PickedMesh;
            public Vector3 PickedPosition;
        }

        public class Result
        {
            public EffectMesh EffectMesh;
            public Vector3 Position;
        }
    }
}
