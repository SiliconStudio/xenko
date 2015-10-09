// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets.Templates
{
    /// <summary>
    /// A template generator.
    /// </summary>
    public interface ITemplateGenerator
    {
        /// <summary>
        /// Determines whether this generator is supporting the specified template
        /// </summary>
        /// <param name="templateDescription">The template description.</param>
        /// <returns><c>true</c> if this generator is supporting the specified template; otherwise, <c>false</c>.</returns>
        bool IsSupportingTemplate(TemplateDescription templateDescription);

        /// <summary>
        /// Prepares this generator with the specified parameters and return a runnable function that must be run just after 
        /// this method. The returned runnable function should return true if it succeeded or false otherwise.
        /// </summary>
        /// <remarks>This method can also return <see langword="null"/> in case the preparation did not complete and nothing
        /// can be further executed.</remarks>
        /// <param name="parameters">The parameters.</param>
        Func<bool> PrepareForRun(TemplateGeneratorParameters parameters);

        /// <summary>
        /// Called only if the generation succeeded.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>True if the method succeeded, False otherwise.</returns>
        bool AfterRun(TemplateGeneratorParameters parameters);
    }


    /// <summary>
    /// Base implementation for <see cref="ITemplateGenerator"/>.
    /// </summary>
    public abstract class TemplateGeneratorBase : ITemplateGenerator
    {
        public abstract bool IsSupportingTemplate(TemplateDescription templateDescription);

        public abstract Func<bool> PrepareForRun(TemplateGeneratorParameters parameters);

        public virtual bool AfterRun(TemplateGeneratorParameters parameters)
        {
            return true;
        }
    }
}