// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows.Markup;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(Guid))]
    public sealed class GuidExtension : MarkupExtension
    {
        public Guid Value { get; set; }

        public GuidExtension()
        {
            Value = Guid.Empty;
        }

        public GuidExtension(object value)
        {
            Guid guid;
            Guid.TryParse(value as string, out guid);
            Value = guid;
        }

        /// <inheritdoc/>
        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}
