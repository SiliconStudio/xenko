// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
