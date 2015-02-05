// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Base implementation for <see cref="IAssetCompiler"/> suitable to build a thumbnail of a single type of <see cref="Asset"/>.
    /// </summary>
    /// <typeparam name="T">Type of the asset</typeparam>
    public abstract class ThumbnailCompilerBase<T> : AssetDependenciesCompilerBase<T> where T : Asset
    {
        private class ThumbnailFailureBuildStep : BuildStep
        {
            public ThumbnailFailureBuildStep(IEnumerable<ILogMessage> messages)
            {
                messages.ForEach(x => Logger.Log(x));
            }

            public override string Title { get { return "FailureThumbnail"; } }

            public override Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
            {
                return Task.FromResult(ResultStatus.Failed);
            }

            public override BuildStep Clone()
            {
                return new ThumbnailFailureBuildStep(Enumerable.Empty<LogMessage>());
            }

            public override string ToString()
            {
                return Title;
            }
        }

        public sealed override AssetCompilerResult Compile(CompilerContext context, AssetItem assetItem)
        {
            // This method is overriden only because of the issue in assignment of the AssetsSession property in the base method.
            // In this implementation, the assignment is deferred in the try/catch block of the CompileOverride method.
            // TODO: Remove this override once the issue in the bas method is fixed (and seal the base method if possible)
            if (context == null) throw new ArgumentNullException("context");
            if (assetItem == null) throw new ArgumentNullException("assetItem");

            Asset = (T)assetItem.Asset;
            AssetItem = assetItem;

            var compilerResult = new AssetCompilerResult();
            CompileOverride((AssetCompilerContext)context, compilerResult);
            return compilerResult;
        }

        protected sealed override AssetCompilerResult CompileOverride(AssetCompilerContext context, AssetCompilerResult compilerResult)
        {
            var thumbnailCompilerContext = (ThumbnailCompilerContext)context;

            // Build the path of the thumbnail in the storage
            var assetStorageUrl = AssetItem.Location.GetDirectoryAndFileName();
            var thumbnailStorageUrl = assetStorageUrl.Insert(0, "__THUMBNAIL__");

            // Check if this asset produced any error
            // (dependent assets errors are generally ignored as long as thumbnail could be generated,
            // but we will add a thumbnail overlay to indicate the state is not good)
            var currentAssetHasErrors = false;

            try
            {
                // TODO: fix failures here (see TODOs in Compile and base.Compile)
                AssetsSession = AssetItem.Package.Session;

                // Only use the path to the asset without its extension
                Compile(thumbnailCompilerContext, thumbnailStorageUrl, AssetItem.FullPath, compilerResult);
            }
            catch (Exception)
            {
                // If an exception occurs, ensure that the build of thumbnail will fail.
                compilerResult.Error(string.Format("An exception occurred while compiling the asset [{0}]", AssetItem.Location));
            }

            foreach (var logMessage in compilerResult.Messages)
            {
                // Ignore anything less than error
                if (!logMessage.IsAtLeast(LogMessageType.Error))
                    continue;

                // Check if there is any non-asset log message
                // (they are probably just emitted by current compiler, so they concern current asset)
                // TODO: Maybe we should wrap every message in AssetLogMessage before copying them in compilerResult?
                var assetLogMessage = logMessage as AssetLogMessage;
                if (assetLogMessage == null)
                {
                    currentAssetHasErrors = true;
                    break;
                }

                // If it was an asset log message, check it concerns current asset
                if (assetLogMessage.AssetReference != null && assetLogMessage.AssetReference.Location == AssetItem.Location)
                {
                    currentAssetHasErrors = true;
                    break;
                }
            }
            if (currentAssetHasErrors)
            {
                // if a problem occurs while compiling, we add a special build step that will always fail.
                compilerResult.BuildSteps.Add(new ThumbnailFailureBuildStep(compilerResult.Messages));
            }

            var currentAsset = AssetItem; // copy the current asset item and embrace it in the callback
            compilerResult.BuildSteps.StepProcessed += (_, buildStepArgs) => OnThumbnailStepProcessed(thumbnailCompilerContext, currentAsset, thumbnailStorageUrl, buildStepArgs);
            return compilerResult;
        }

        private static void OnThumbnailStepProcessed(ThumbnailCompilerContext context, AssetItem assetItem, string thumbnailStorageUrl, BuildStepEventArgs buildStepEventArgs)
        {
            // returns immediately if the user has not subscribe to the event
            if (!context.ShouldNotifyThumbnailBuilt)
                return;

            // TODO: the way to get last build step (which should be thumbnail, not its dependencies) should be done differently, at the compiler level
            // (we need to generate two build step that can be accessed directly, one for dependency and one for thumbnail)
            var lastBuildStep = buildStepEventArgs.Step is ListBuildStep ? ((ListBuildStep)buildStepEventArgs.Step).LastOrDefault() ?? buildStepEventArgs.Step : buildStepEventArgs.Step;

            // Retrieving build result
            var result = ThumbnailBuildResult.Failed;
            if (lastBuildStep.Succeeded)
                result = ThumbnailBuildResult.Succeeded;
            else if (lastBuildStep.Status == ResultStatus.Cancelled)
                result = ThumbnailBuildResult.Cancelled;

            // TODO: Display error logo if anything else went wrong?

            var changed = lastBuildStep.Status != ResultStatus.NotTriggeredWasSuccessful;

            // Open the image data stream if the build succeeded
            Stream thumbnailStream = null;
            ObjectId thumbnailHash = ObjectId.Empty;

            if (lastBuildStep.Succeeded)
            {
                thumbnailStream = AssetManager.FileProvider.OpenStream(thumbnailStorageUrl, VirtualFileMode.Open, VirtualFileAccess.Read);
                thumbnailHash = AssetManager.FileProvider.AssetIndexMap[thumbnailStorageUrl];
            }

            try
            {
                context.NotifyThumbnailBuilt(assetItem, result, changed, thumbnailStream, thumbnailHash);
            }
            finally
            {
                // Close the image data stream if opened
                if (thumbnailStream != null)
                {
                    thumbnailStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Compiles the asset from the specified package.
        /// </summary>
        /// <param name="context">The thumbnail compile context</param>
        /// <param name="thumbnailStorageUrl">The absolute URL to the asset's thumbnail, relative to the storage.</param>
        /// <param name="assetAbsolutePath">Absolute path of the asset on the disk</param>
        /// <param name="result">The result where the commands and logs should be output.</param>
        protected abstract void Compile(ThumbnailCompilerContext context, string thumbnailStorageUrl, UFile assetAbsolutePath, AssetCompilerResult result);
    }
}