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
        /// Prepares this generator with the specified parameters and return a runnable action that must be run just after 
        /// this method.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        Action PrepareForRun(TemplateGeneratorParameters parameters);

        void AfterRun(TemplateGeneratorParameters parameters);
    }


    /// <summary>
    /// Base implementation for <see cref="ITemplateGenerator"/>.
    /// </summary>
    public abstract class TemplateGeneratorBase : ITemplateGenerator
    {
        public abstract bool IsSupportingTemplate(TemplateDescription templateDescription);

        public abstract Action PrepareForRun(TemplateGeneratorParameters parameters);

        public virtual void AfterRun(TemplateGeneratorParameters parameters)
        {
        }
    }
}