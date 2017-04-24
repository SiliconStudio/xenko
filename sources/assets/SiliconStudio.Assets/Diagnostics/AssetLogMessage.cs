// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Yaml;

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
        /// Initializes a new instance of the <see cref="AssetLogMessage" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="assetReference">The asset reference.</param>
        /// <param name="type">The type.</param>
        /// <param name="messageCode">The message code.</param>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="System.ArgumentNullException">asset</exception>
        public AssetLogMessage(Package package, IReference assetReference, LogMessageType type, string text)
        {
            this.package = package;
            AssetReference = assetReference;
            Type = type;
            Related = new List<IReference>();
            Text = text;
        }

        public static AssetLogMessage From(Package package, IReference assetReference, ILogMessage logMessage, string assetPath, int line = 0, int character = 0)
        {
            // Transform to AssetLogMessage
            var assetLogMessage = logMessage as AssetLogMessage;
            if (assetLogMessage == null)
            {
                assetLogMessage = new AssetLogMessage(null, assetReference, logMessage.Type, AssetMessageCode.CompilationMessage, assetReference?.Location, logMessage.Text)
                {
                    Exception = (logMessage as LogMessage)?.Exception
                };
            }

            // Set file (and location if available)
            assetLogMessage.File = assetPath;
            assetLogMessage.Line = line;
            assetLogMessage.Character = character;

            // Generate location (if it's a Yaml exception)
            var yamlException = (logMessage as LogMessage)?.Exception as YamlException;
            if (yamlException != null)
            {
                assetLogMessage.Line = yamlException.Start.Line;
                assetLogMessage.Character = yamlException.Start.Column;
                // We've already got everything, no need to pollute log with stack trace of exception
                assetLogMessage.Exception = null;
            }

            return assetLogMessage;
        }

        public string File { get; set; }

        public int Line { get; set; }

        public int Character { get; set; }

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

        public override string ToString()
        {
            var result = base.ToString();
            if (AssetReference?.Location != null)
                result = $"{AssetReference.Location}({Line + 1},{Character + 1}): {result}";

            return result;
        }
    }
}
