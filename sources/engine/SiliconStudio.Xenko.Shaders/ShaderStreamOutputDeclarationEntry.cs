// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Description of a StreamOutput declaration entry.
    /// </summary>
    [DataContract]
    public struct ShaderStreamOutputDeclarationEntry
    {
        /// <summary>
        /// The stream index.
        /// </summary>
        public int Stream;

        /// <summary>
        /// The semantic name.
        /// </summary>
        public string SemanticName;

        /// <summary>
        /// The semantic index.
        /// </summary>
        public int SemanticIndex;

        /// <summary>
        /// The start component
        /// </summary>
        public byte StartComponent;

        /// <summary>
        /// The component count
        /// </summary>
        public byte ComponentCount;

        /// <summary>
        /// The output slot
        /// </summary>
        public byte OutputSlot;
    }
}