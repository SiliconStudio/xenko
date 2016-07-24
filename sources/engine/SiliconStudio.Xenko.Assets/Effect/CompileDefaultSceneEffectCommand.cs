using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Assets.Effect
{
    /// <summary>
    /// Loads the scene and force effects to compile.
    /// </summary>
    public class CompileDefaultSceneEffectCommand : IndexFileCommand
    {
        private readonly AssetCompilerContext context;
        private readonly Package package;
        private readonly AssetCompilerResult compilerResult;

        public override string Title => "Compiling scene effects";

        public CompileDefaultSceneEffectCommand(AssetCompilerContext context, Package package, AssetCompilerResult compilerResult)
        {
            this.context = context;
            this.package = package;
            this.compilerResult = compilerResult;
        }

        protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
        {
            // Add game settings asset URL
            yield return new ObjectUrl(UrlType.Content, GameSettingsAsset.GameSettingsLocation);

            var gameSettings = context.GetGameSettingsAsset();

            var defaultSceneUrl = gameSettings.DefaultScene != null ? AttachedReferenceManager.GetUrl(gameSettings.DefaultScene) : null;
            if (defaultSceneUrl == null)
                yield break;

            // Add scene
            yield return new ObjectUrl(UrlType.Content, defaultSceneUrl);

            // And all its dependencies (TODO: restrict ourselves on what affect rendering?)
            var sceneAssetItem = package.Session.FindAsset(defaultSceneUrl);
            var dependencies = package.Session.DependencyManager.ComputeDependencies(sceneAssetItem, AssetDependencySearchOptions.Out | AssetDependencySearchOptions.Recursive, ContentLinkType.Reference);

            foreach (var dependency in dependencies.LinksOut)
            {
                yield return new ObjectUrl(UrlType.Content, dependency.Item.Location);
            }
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);

            // Regenerate new shaders if magic header changed
            uint effectbyteCodeMagicNumber = EffectBytecode.MagicHeader;
            writer.Serialize(ref effectbyteCodeMagicNumber, ArchiveMode.Serialize);
        }

        /// <inheritdoc/>
        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var gameSettings = context.GetGameSettingsAsset();

            // Find default scene URL
            var defaultSceneUrl = gameSettings.DefaultScene != null ? AttachedReferenceManager.GetUrl(gameSettings.DefaultScene) : null;
            if (defaultSceneUrl == null)
                return Task.FromResult(ResultStatus.Successful);

            var baseUrl = new UFile(defaultSceneUrl).GetParent();

            try
            {
                commandContext.Logger.Info($"Trying to compile effects for scene '{defaultSceneUrl}'");

                using (var sceneRenderer = new SceneRenderer(gameSettings))
                {
                    // Effect can be compiled asynchronously (since we don't have any fallback, they will have to be compiled in the same frame anyway)
                    // Also set the file provider to the current transaction
                    ((EffectCompilerCache)sceneRenderer.EffectSystem.Compiler).CompileEffectAsynchronously = true;
                    ((EffectCompilerCache)sceneRenderer.EffectSystem.Compiler).FileProvider = DatabaseFileProvider;
                    ((EffectCompilerCache)sceneRenderer.EffectSystem.Compiler).CurrentCache = EffectBytecodeCacheLoadSource.StartupCache;
                    sceneRenderer.EffectSystem.EffectUsed += (effectCompileRequest, result) => compilerResult.BuildSteps.Add(EffectCompileCommand.FromRequest(context, package, baseUrl, effectCompileRequest));

                    sceneRenderer.GameSystems.LoadContent();

                    // Load the scene
                    var scene = sceneRenderer.ContentManager.Load<Scene>(defaultSceneUrl);
                    sceneRenderer.SceneSystem.SceneInstance = new SceneInstance(sceneRenderer.Services, scene, ExecutionMode.EffectCompile);

                    // Disable culling
                    sceneRenderer.SceneSystem.SceneInstance.VisibilityGroups.CollectionChanged += (sender, e) =>
                    {
                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                        {
                            ((VisibilityGroup)e.Item).DisableCulling = true;
                        }
                    };

                    // Update and draw
                    // This will force effects to be generated and saved in the object database
                    var time = new GameTime();
                    sceneRenderer.GameSystems.Update(time);
                    sceneRenderer.GraphicsContext.ResourceGroupAllocator.Reset();
                    sceneRenderer.GameSystems.Draw(time);
                }
            }
            catch (Exception e)
            {
                commandContext.Logger.Warning($"Could not compile effects for scene '{defaultSceneUrl}': {e.Message + e.StackTrace}", e);
            }

            return Task.FromResult(ResultStatus.Successful);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}