// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SharpDX;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.ComputeEffect.GGXPrefiltering;
using SiliconStudio.Paradox.Effects.ComputeEffect.LambertianPrefiltering;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.GeometricPrimitives
{
    internal class GeometricPrimitiveAssetCompiler : AssetCompilerBase<GeometricPrimitiveAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, GeometricPrimitiveAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new ListBuildStep { new GeometricPrimitiveCompileCommand(urlInStorage, asset, context.Package) };
        }

        private class GeometricPrimitiveCompileCommand : AssetCommand<GeometricPrimitiveAsset>
        {
            public GeometricPrimitiveCompileCommand(string url, GeometricPrimitiveAsset asset, Package package)
                : base(url, asset)
            {
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
                writer.Write(1); // Change this number to recompute the hash when prefiltering algorithm are changed
            }

            protected unsafe override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                if (asset.Model == null)
                {
                    throw new InvalidOperationException("Invalid GeometricPrimitive [{0}]. Expecting a non-null Type");
                }

                var data = asset.Model.Create();

                if (data.Vertices.Length == 0)
                {
                    throw new InvalidOperationException("Invalid GeometricPrimitive [{0}]. Expecting non-zero Vertices array");
                }

                var layout = data.Vertices[0].GetLayout();

                fixed (void* indexBuffer = data.Indices)
                fixed (void* vertexBuffer = data.Vertices)
                {
                    var result = TNBExtensions.GenerateTangentBinormal(layout, (IntPtr)vertexBuffer, data.Vertices.Length, 0, layout.VertexStride, (IntPtr)indexBuffer, true, data.Indices.Length);

                    var model = new Model();
                    var mesh = new Mesh { Draw = new MeshDraw { VertexBuffers = new VertexBufferBinding[1] } };

                    var vertexBufferData = new BufferData(BufferFlags.VertexBuffer, result.Value);
                    var indexBufferData = BufferData.New(BufferFlags.IndexBuffer, data.Indices);

                    mesh.Draw.VertexBuffers[0] = new VertexBufferBinding(
                        vertexBufferData.ToSerializableVersion(),
                        layout,
                        data.Vertices.Length);

                    mesh.Draw.IndexBuffer = new IndexBufferBinding(indexBufferData.ToSerializableVersion(), true, data.Indices.Length, 0);
                    mesh.Draw.PrimitiveType = PrimitiveType.TriangleList;
                    mesh.Draw.DrawCount = data.Indices.Length;
                    model.Meshes.Add(mesh);

                    if (asset.Material != null)
                    {
                        var material = AttachedReferenceManager.CreateSerializableVersion<Material>(asset.Material);
                        model.Materials.Add(material);
                    }

                    var assetManager = new AssetManager();
                    assetManager.Save(Url, model);
                }

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
