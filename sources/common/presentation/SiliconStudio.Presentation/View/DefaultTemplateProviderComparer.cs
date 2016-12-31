// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.View
{
    /// <summary>
    /// A default implementation of the <see cref="TemplateProviderComparerBase"/> class that compares <see cref="ITemplateProvider"/> instances by name.
    /// </summary>
    public class DefaultTemplateProviderComparer : TemplateProviderComparerBase
    {
        protected override int CompareProviders([NotNull] ITemplateProvider x, [NotNull] ITemplateProvider y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }
}