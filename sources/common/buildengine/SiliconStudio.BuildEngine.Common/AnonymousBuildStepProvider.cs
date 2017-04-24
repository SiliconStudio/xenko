// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// An implementation of the <see cref="IBuildStepProvider"/> interface that allows to create a build step provider
    /// from an anonymous function.
    /// </summary>
    public class AnonymousBuildStepProvider : IBuildStepProvider
    {
        private readonly Func<int, BuildStep> providerFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousBuildStepProvider"/> class.
        /// </summary>
        /// <param name="providerFunction">The function that provides build steps.</param>
        public AnonymousBuildStepProvider(Func<int, BuildStep> providerFunction)
        {
            if (providerFunction == null) throw new ArgumentNullException("providerFunction");
            this.providerFunction = providerFunction;
        }

        /// <inheritdoc/>
        public BuildStep GetNextBuildStep(int maxPriority)
        {
            return providerFunction(maxPriority);
        }
    }
}
