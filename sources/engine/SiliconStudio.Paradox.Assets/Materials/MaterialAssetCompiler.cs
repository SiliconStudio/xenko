// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Effects.Data;
using System.Linq;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Materials
{
    internal class MaterialAssetCompiler : AssetCompilerBase<MaterialAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, MaterialAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new ListBuildStep { new MaterialCompileCommand(urlInStorage, AssetItem, asset, context) };
        }

        private class MaterialCompileCommand : AssetCommand<MaterialAsset>
        {
            private readonly AssetItem assetItem;

            private readonly Package package;

            private UFile assetUrl;

            public MaterialCompileCommand(string url, AssetItem assetItem, MaterialAsset value, AssetCompilerContext context)
                : base(url, value)
            {
                this.assetItem = assetItem;
                package = context.Package;
                assetUrl = new UFile(url);
            }

            public override System.Collections.Generic.IEnumerable<ObjectUrl> GetInputFiles()
            {
                foreach (var materialReference in asset.FindMaterialReferences())
                {
                    yield return new ObjectUrl(UrlType.Internal, materialReference.Location);
                }

                // TODO: Add textures when we will bake them

                //var materialTextureVisitor = new MaterialTextureVisitor(asset.Material);
                //foreach (var textureLocation in materialTextureVisitor.GetAllTextureValues().Where(IsTextureReferenceValid).Select(x => x.TextureReference.Location).Distinct())
                //    yield return new ObjectUrl(UrlType.Internal, textureLocation);
                foreach (var inputFile in base.GetInputFiles())
                    yield return inputFile;
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
                writer.Serialize(ref assetUrl, ArchiveMode.Serialize);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // Reduce trees on CPU
                //var materialReducer = new MaterialTreeReducer(material);
                //materialReducer.ReduceTrees();

                //foreach (var reducedTree in materialReducer.ReducedTrees)
                //{
                //    material.Nodes[reducedTree.Key] = reducedTree.Value;
                //}

                // Reduce on GPU 
                // TODO: Adapt GPU reduction so that it is compatible Android color/alpha separation
                // TODO: Use the build engine processed output textures instead of the imported one (not existing any more)
                // TODO: Set the reduced texture output format
                // TODO: The graphics device cannot be shared with the Previewer
                //var graphicsDevice = (GraphicsDevice)context.Attributes.GetOrAdd(CompilerContext.GraphicsDeviceKey, key => GraphicsDevice.New(DeviceCreationFlags.None, GraphicsProfile.Level_11_0));
                //using (var materialTextureLayerFlattener = new MaterialTextureLayerFlattener(material, graphicsDevice))
                //{
                //    materialTextureLayerFlattener.PrepareForFlattening(new UDirectory(assetUrl.Directory));
                //    if (materialTextureLayerFlattener.HasCommands)
                //    {
                //        var compiler = EffectCompileCommand.GetOrCreateEffectCompiler(context);
                //        materialTextureLayerFlattener.Run(compiler);
                //        // store Material with modified textures
                //        material = materialTextureLayerFlattener.Material;
                //    }
                //}

                var materialContext = new MaterialGeneratorContext(package);
                var materialClone = (MaterialAsset)AssetCloner.Clone(Asset);
                var result = MaterialGenerator.Generate(materialClone, materialContext);

                if (result.HasErrors)
                {
                    result.CopyTo(commandContext.Logger);
                    return Task.FromResult(ResultStatus.Failed);
                }
                // Separate the textures into color/alpha components on Android to be able to use native ETC1 compression
                //if (context.Platform == PlatformType.Android)
                //{
                //    var alphaComponentSplitter = new TextureAlphaComponentSplitter(assetItem.Package.Session);
                //    material = alphaComponentSplitter.Run(material, new UDirectory(assetUrl.GetDirectory())); // store Material with alpha substituted textures
                //}

                // Create the parameters
                //var materialParameterCreator = new MaterialParametersCreator(material, assetUrl);
                //if (materialParameterCreator.CreateParameterCollectionData(commandContext.Logger))
                //    return Task.FromResult(ResultStatus.Failed);

                var materialData = new Material { Parameters = materialContext.Parameters};
                
                var assetManager = new AssetManager();
                assetManager.Save(assetUrl, materialData);

                return Task.FromResult(ResultStatus.Successful);
            }
            
            public override string ToString()
            {
                return (assetUrl ?? "[File]") + " (Material) > " + (assetUrl ?? "[Location]");
            }
        }
    }
}
 
