// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Xenko.Shaders.Parser
{
    /// <summary>
    /// Results of a <see cref="ShaderNavigation"/>
    /// </summary>
    public class ShaderNavigationResult
    {
        public ShaderNavigationResult()
        {
            Messages = new LoggerResult();
        }

        /// <summary>
        /// Gets or sets the definition location.
        /// </summary>
        /// <value>The definition location.</value>
        public SiliconStudio.Shaders.Ast.SourceSpan DefinitionLocation { get; set; }

        /// <summary>
        /// Gets the parsing messages.
        /// </summary>
        /// <value>The messages.</value>
        public LoggerResult Messages { get; set; }
    }
}
