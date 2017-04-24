// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// This interface describes a class that is capable of providing build steps to a <see cref="DynamicBuildStep"/>.
    /// </summary>
    public interface IBuildStepProvider
    {
        /// <summary>
        /// Gets the next build step to execute.
        /// </summary>
        /// <returns>The next build step to execute, or <c>null</c> if there is no build step to execute.</returns>
        BuildStep GetNextBuildStep(int maxPriority);
    }
}
