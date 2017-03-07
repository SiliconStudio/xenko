// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.View
{
    /// <summary>
    /// A default implementation of the <see cref="ITemplateProvider"/> interface that matches any object.
    /// </summary>
    public class DefaultTemplateProvider : TemplateProviderBase
    {
        /// <inheritdoc/>
        public override string Name => "Default";

        /// <inheritdoc/>
        public override bool Match(object obj)
        {
            return true;
        }
    }
}
