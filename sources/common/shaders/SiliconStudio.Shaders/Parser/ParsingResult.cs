// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Shaders.Parser
{

    /// <summary>
    /// A Parsing result.
    /// </summary>
    [DataContract]
    public class ParsingResult : LoggerResult
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the shader.
        /// </summary>
        /// <value>
        /// The shader.
        /// </value>
        public Shader Shader { get; set; }

        /// <summary>
        /// Gets or sets the time to parse in ms.
        /// </summary>
        /// <value>
        /// The time to parse ms.
        /// </value>
        public long TimeToParse { get; set; }

        /// <summary>
        /// Gets or sets the token count.
        /// </summary>
        /// <value>
        /// The token count.
        /// </value>
        public int TokenCount { get; set; }

        #endregion
    }
}