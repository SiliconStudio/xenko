// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace NShader
{
    /// <summary>
    /// A log message for a particular line.
    /// </summary>
    [Serializable]
    public class RawShaderAnalysisMessage
    {
        /// <summary>
        /// Gets or sets the Span.
        /// </summary>
        /// <value>The Span.</value>
        public RawSourceSpan Span { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2} : {3}", this.Span, Type, this.Code, this.Text);
        }
    }
}
