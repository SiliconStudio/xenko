// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.BuildEngine;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A thumbnail list compiler.
    /// This compiler creates the list of build steps to perform to produce the thumbnails of an list of <see cref="AssetItem"/>.
    /// </summary>
    public class ThumbnailListCompiler : ItemListCompiler, IBuildStepProvider
    {
        private static readonly ThumbnailCompilerRegistry ThumbnailCompilerRegistry = new ThumbnailCompilerRegistry();

        private readonly ThumbnailCompilerContext context;

        private Func<AssetItem> getNextThumbnailToBuild;
        private readonly object internObjectsLock = new object();

        private readonly AssetCompilerResult compilationResult = new AssetCompilerResult();
        
        /// <summary>
        /// A list of queued build step already generated waiting to be executed.
        /// </summary>
        private readonly Queue<BuildStep> nextBuildSteps = new Queue<BuildStep>();
        
        /// <summary>
        /// Creates an instance of <see cref="ThumbnailListCompiler"/>.
        /// </summary>
        public ThumbnailListCompiler(ThumbnailCompilerContext context)
            : base(ThumbnailCompilerRegistry)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;
        }

        /// <summary>
        /// Gets or sets the handler providing the list of items to build to the compiler
        /// </summary>
        public Func<AssetItem> GetNextThumbnailToBuild
        {
            get { return getNextThumbnailToBuild; }
            set
            {
                lock (internObjectsLock)
                    getNextThumbnailToBuild = value;
            }
        }

        /// <summary>
        /// Get the next build step to execute to compile the thumbnail.
        /// </summary>
        /// <returns></returns>
        public BuildStep GetNextBuildStep()
        {
            // dequeue previously needed build steps
            if (nextBuildSteps.Count > 0)
                return nextBuildSteps.Dequeue();

            // get the next asset item to compile
            AssetItem nextItem = null;
            lock (internObjectsLock)
            {
                if (GetNextThumbnailToBuild != null)
                    nextItem = GetNextThumbnailToBuild();
            }
            
            // add the compilation steps to the queue
            if (nextItem != null)
            {
                // add the compilation of the thumbnail itself
                nextBuildSteps.Enqueue(CompileItem(context, compilationResult, nextItem));
            }

            // return the new first build step if any
            if (nextBuildSteps.Count > 0)
                return nextBuildSteps.Dequeue();

            return null; // no work to do for the moment
        }

        /// <summary>
        /// Register a default compiler to use when no compiler is explicitly associated to an asset type.
        /// </summary>
        /// <param name="compiler">The compiler to use as default compiler (can be null)</param>
        public static void RegisterDefaultThumbnailCompiler(IAssetCompiler compiler)
        {
            ThumbnailCompilerRegistry.DefaultCompiler = compiler;
        }
    }
}
