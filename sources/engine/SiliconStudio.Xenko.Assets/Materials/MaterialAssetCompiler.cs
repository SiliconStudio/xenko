// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Assets.Materials
{
    internal class MaterialAssetCompiler : AssetCompilerBase
    {
        public MaterialAssetCompiler()
        {
            CompileTimeDependencyTypes.Add(typeof(TextureAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent); //textures
            CompileTimeDependencyTypes.Add(typeof(MaterialAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent); //sub-materials
        }

        public override IEnumerable<ObjectUrl> GetInputFiles(AssetItem assetItem)
        {
            foreach (var compileTimeDependency in ((MaterialAsset)assetItem.Asset).EnumerateCompileTimeDependencies(assetItem.Package.Session))
            {
                yield return new ObjectUrl(UrlType.ContentLink, compileTimeDependency.Location);
            }
        }

        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (MaterialAsset)assetItem.Asset;
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new AssetBuildStep(assetItem) { new MaterialCompileCommand(targetUrlInStorage, assetItem, asset, context) };
        }

        private class MaterialCompileCommand : AssetCommand<MaterialAsset>
        {
            private readonly AssetItem assetItem;

            private ColorSpace colorSpace;

            private UFile assetUrl;

            public MaterialCompileCommand(string url, AssetItem assetItem, MaterialAsset value, AssetCompilerContext context)
                : base(url, value, assetItem.Package)
            {
                this.assetItem = assetItem;
                colorSpace = context.GetColorSpace();
                assetUrl = new UFile(url);
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                // TODO: Add textures when we will bake them
                foreach (var compileTimeDependency in ((MaterialAsset)assetItem.Asset).EnumerateCompileTimeDependencies(Package.Session))
                {
                    yield return new ObjectUrl(UrlType.ContentLink, compileTimeDependency.Location);
                }
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);

                writer.Serialize(ref assetUrl, ArchiveMode.Serialize);

                // Write the 
                writer.Write(colorSpace);
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

                // Check with Ben why DoCommandOverride is called without going through the constructor?
                var assetManager = new ContentManager();
                var materialContext = new MaterialGeneratorContext
                {
                    Content = assetManager,
                    ColorSpace = colorSpace
                };
                materialContext.AddLoadingFromSession(Package);

                var materialClone = AssetCloner.Clone(Parameters);
                var result = MaterialGenerator.Generate(new MaterialDescriptor() { MaterialId = materialClone.Id, Attributes = materialClone.Attributes, Layers = materialClone.Layers}, materialContext, string.Format("{0}:{1}", materialClone.Id, assetUrl));

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

                assetManager.Save(assetUrl, result.Material);

                return Task.FromResult(ResultStatus.Successful);
            }
            
            public override string ToString()
            {
                return (assetUrl ?? "[File]") + " (Material) > " + (assetUrl ?? "[Location]");
            }
        }
    }
}
 
