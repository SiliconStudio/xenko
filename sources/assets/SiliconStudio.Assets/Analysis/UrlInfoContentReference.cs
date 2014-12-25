// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Implements <see cref="IContentReference"/> for a <see cref="UrlServices.UrlInfo"/>.
    /// </summary>
    internal class UrlInfoContentReference : IContentReference
    {
        private readonly UrlServices.UrlInfo urlInfo;

        /// <inheritdoc/>
        public Guid Id { get { return urlInfo.Id; } }

        /// <inheritdoc/>
        public string Location { get { return urlInfo.Url; } }

        public UrlInfoContentReference(UrlServices.UrlInfo urlInfo)
        {
            this.urlInfo = urlInfo;
        }
    }
}