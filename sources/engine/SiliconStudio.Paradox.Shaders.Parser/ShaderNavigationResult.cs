// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Paradox.Shaders.Parser
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