// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An event fired when the <see cref="Asset.Base"/> is changed.
    /// </summary>
    public class AssetBaseChangedEvent : EventArgs
    {
        private readonly AssetBase previousBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBaseChangedEvent"/> class.
        /// </summary>
        /// <param name="previousBase">The previous reference.</param>
        public AssetBaseChangedEvent(AssetBase previousBase)
        {
            this.previousBase = previousBase;
        }

        /// <summary>
        /// Gets the previous base reference of an asset.
        /// </summary>
        /// <value>The previous reference.</value>
        public AssetBase PreviousBase
        {
            get
            {
                return previousBase;
            }
        }
    }
}