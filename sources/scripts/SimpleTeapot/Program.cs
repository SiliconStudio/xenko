// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xenko;
using Xenko.Effects;
using Xenko.Effects.Modules;
using Xenko.Framework;
using Xenko.Framework.Graphics;
using Xenko.Framework.Mathematics;
using Xenko.Framework.Serialization;
using Xenko.Framework.VirtualFileSystem;
using Xenko.Framework.Serialization.Contents;
using Xenko.Framework.Graphics.Data;

namespace SimpleTeapot
{
    class Program
    {
        public class Win32Interop
        {
            [DllImport("crtdll.dll")]
            public static extern int _kbhit();
        }

        static void Main(string[] args)
        {
            EngineContext.Setup();
            EngineContext.RenderSystem = new DefaultRenderSystem();
            EngineContext.RenderSystem.Init(EngineContext.RenderContext);

            var effect = EngineContext.RenderContext.BuildEffect().Using<RenderTargetFeature>().Using<TransformationFeature>().Using<TextureFeature>().Compile();

            var vfs = new VirtualFileStorage();
            vfs.MountFileSystem("/global_data", "..\\deps\\data\\");
            vfs.MountFileSystem("/global_data2", "..\\data\\");
            var packageVfs = vfs.MountPackage("/testpackage", "/global_data/factory3.dat").Result;
            var contentManager = new ContentManager(vfs, packageVfs);
            contentManager.RegisterSerializer(new SimpleContentSerializer<MeshData>());
            contentManager.RegisterSerializer(new TextureSerializer());
            contentManager.RegisterSerializer(new GpuTextureSerializer(EngineContext.RenderContext.GraphicsDevice));

            var meshData = contentManager.Load<MeshData>("/testpackage/guid/" + packageVfs.Objects[2].Header.ObjectId.Guid);
            var effectMesh = new EffectMesh(effect, meshData);
            //effect.InstantiateMesh(effectMesh);
            //effect.UpdateMeshData(effectMesh, meshData);
            //effectMesh.AddRenderQueue();

            var projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI * 0.5f, 1024.0f / 768.0f, 1.0f, 4000.0f);

            EngineContext.RenderContext.ParameterGroups.Get(TransformationFeature.TransformationGroup).SetAs(TransformationFeature.Projection, projectionMatrix);

            var factoryTexture = (ITexture2D)contentManager.Load<ITexture>("/global_data2/media/factory.png");
            effectMesh.Parameters.Set(TextureFeature.Texture, factoryTexture);

            var world = Matrix.Scaling(0.1f) * Matrix.Translation(new Vector3(-30.0f, -30.0f, 0.0f));
            effectMesh.Parameters.Set(TransformationFeature.World, world);

            var depth = EngineContext.RenderContext.GraphicsDevice.DepthStencilBuffer.New(DepthFormat.Depth32, EngineContext.RenderContext.Width, EngineContext.RenderContext.Height);
            EngineContext.RenderContext.ParameterGroups.GetGroup(RenderTargetFeature.Group).Set(RenderTargetFeature.RenderTarget, EngineContext.RenderContext.RenderTarget);
            EngineContext.RenderContext.ParameterGroups.GetGroup(RenderTargetFeature.Group).Set(RenderTargetFeature.DepthStencil, depth);

            float time = 0.0f;

            while (true)
            {
                var eyeVector = new Vector4(-800.0f * (float)Math.Cos(time), 800.0f * (float)Math.Sin(time), 500.0f, 1.0f);
                time += 0.0001f;
                var viewMatrix = Matrix.LookAtLH(new Vector3(eyeVector.X, eyeVector.Y, eyeVector.Z), new Vector3(0.0f, 0.0f, 50.0f), new Vector3(0.0f, 0.0f, 1.0f));
                EngineContext.RenderContext.ParameterGroups.Get(TransformationFeature.TransformationGroup).SetAs(TransformationFeature.View, viewMatrix);
                
                Scheduler.Step();
                WinFormsHelper.UpdateWindow();
                EngineContext.Render();

                if (Win32Interop._kbhit() != 0)
                {
                    var key = Console.ReadKey(true).KeyChar;
                    switch (key)
                    {
                        default:
                            break;
                    }
                    if (key == 'q')
                        break;
                }
            }
            EngineContext.Stop();

        }
    }
}
