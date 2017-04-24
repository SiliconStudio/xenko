// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;

namespace SiliconStudio.Assets.Templates
{
    /// <summary>
    /// Base implementation for <see cref="ITemplateGenerator"/> and <see cref="ITemplateGenerator{TParameters}"/>.
    /// </summary>
    /// <typeparam name="TParameters">The type of parameters this generator uses.</typeparam>
    public abstract class TemplateGeneratorBase<TParameters> : ITemplateGenerator<TParameters> where TParameters : TemplateGeneratorParameters
    {
        /// <inheritdoc/>
        public abstract bool IsSupportingTemplate(TemplateDescription templateDescription);

        /// <inheritdoc/>
        public abstract Task<bool> PrepareForRun(TParameters parameters);

        /// <inheritdoc/>
        public abstract bool Run(TParameters parameters);
    }
}
