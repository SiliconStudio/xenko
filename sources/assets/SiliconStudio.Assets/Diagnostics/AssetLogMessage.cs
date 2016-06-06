// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Diagnostics
{
    /// <summary>
    /// Provides a specialized <see cref="LogMessage"/> to give specific information about an asset.
    /// </summary>
    public class AssetLogMessage : LogMessage
    {
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetLogMessage" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="assetReference">The asset reference.</param>
        /// <param name="type">The type.</param>
        /// <param name="messageCode">The message code.</param>
        /// <exception cref="System.ArgumentNullException">asset</exception>
        public AssetLogMessage(Package package, IReference assetReference, LogMessageType type, AssetMessageCode messageCode)
        {
            this.package = package;
            AssetReference = assetReference;
            Type = type;
            MessageCode = messageCode;
            Related = new List<IReference>();
            Text = AssetMessageStrings.ResourceManager.GetString(messageCode.ToString()) ?? messageCode.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetLogMessage" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="assetReference">The asset reference.</param>
        /// <param name="type">The type.</param>
        /// <param name="messageCode">The message code.</param>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="System.ArgumentNullException">asset</exception>
        public AssetLogMessage(Package package, IReference assetReference, LogMessageType type, AssetMessageCode messageCode, params object[] arguments)
        {
            this.package = package;
            AssetReference = assetReference;
            Type = type;
            MessageCode = messageCode;
            Related = new List<IReference>();
            var message = AssetMessageStrings.ResourceManager.GetString(messageCode.ToString()) ?? messageCode.ToString();
            Text = string.Format(message, arguments);
        }

        /// <summary>
        /// Gets or sets the message code.
        /// </summary>
        /// <value>The message code.</value>
        public AssetMessageCode MessageCode { get; set; }

        /// <summary>
        /// Gets or sets the asset this message applies to (optional).
        /// </summary>
        /// <value>The asset.</value>
        public IReference AssetReference { get; set; }

        /// <summary>
        /// Gets or sets the package.
        /// </summary>
        /// <value>The package.</value>
        public Package Package { get { return package; } }

        /// <summary>
        /// Gets or sets the member of the asset this message applies to. May be null.
        /// </summary>
        /// <value>The member.</value>
        public IMemberDescriptor Member { get; set; }

        /// <summary>
        /// Gets or sets the related references.
        /// </summary>
        /// <value>The related.</value>
        public List<IReference> Related { get; private set; }
    }
}
