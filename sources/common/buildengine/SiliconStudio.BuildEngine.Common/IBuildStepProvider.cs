// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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