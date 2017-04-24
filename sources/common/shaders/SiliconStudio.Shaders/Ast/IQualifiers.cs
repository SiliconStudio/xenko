// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Base interface for all node providing qualifiers.
    /// </summary>
    public interface IQualifiers
    {
        /// <summary>
        /// Gets or sets the qualifiers.
        /// </summary>
        /// <value>
        /// The qualifiers.
        /// </value>
        Qualifier Qualifiers { get; set; }
    }
}
