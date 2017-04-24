// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Templates
{
    /// <summary>
    /// Describes if a template is supporting a particular context
    /// </summary>
    [DataContract("TemplateScope")]
    public enum TemplateScope
    {
        // TODO We could use flags instead

        /// <summary>
        /// The template can be applied to an existing <see cref="PackageSession"/>.
        /// </summary>
        Session,

        /// <summary>
        /// The template can be applied to an existing <see cref="Assets.Package"/>.
        /// </summary>
        Package,

        /// <summary>
        /// The template can be applied to certain types of Assets <see cref="Assets.Asset"/>.
        /// </summary>
        Asset
    }
}
