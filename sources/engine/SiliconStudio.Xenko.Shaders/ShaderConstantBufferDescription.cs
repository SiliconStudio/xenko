// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Description of a constant buffer.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("[{Stage}] cbuffer {Name} : {Size} bytes")]
    public class ShaderConstantBufferDescription
    {
        /// <summary>
        /// The name of this constant buffer.
        /// </summary>
        public string Name;

        /// <summary>
        /// The size in bytes.
        /// </summary>
        public int Size;

        /// <summary>
        /// The type of constant buffer.
        /// </summary>
        public ConstantBufferType Type;

        /// <summary>
        /// The stage from where this constant buffer is used.
        /// </summary>
        public ShaderStage Stage;

        /// <summary>
        /// The members of this constant buffer.
        /// </summary>
        public EffectParameterValueData[] Members;

        [DataMemberIgnore]
        public ObjectId Hash;

        /// <summary>
        /// Clone the current instance of the constant buffer description.
        /// </summary>
        /// <returns>A clone copy of the description</returns>
        public ShaderConstantBufferDescription Clone()
        {
            return (ShaderConstantBufferDescription)MemberwiseClone();
        }
    }
}