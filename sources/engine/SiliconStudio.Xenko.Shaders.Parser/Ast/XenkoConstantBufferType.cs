// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast.Hlsl;

namespace SiliconStudio.Xenko.Shaders.Parser.Ast
{
    public class XenkoConstantBufferType : SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType
    {
        /// <summary>
        ///   Resource group keyword (rgroup).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType ResourceGroup = new XenkoConstantBufferType("rgroup");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<XenkoConstantBufferType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XenkoStorageQualifier"/> class.
        /// </summary>
        public XenkoConstantBufferType()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="XenkoStorageQualifier"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        public XenkoConstantBufferType(string key)
            : base(key)
        {
        }

        /// <summary>
        /// Parses the specified enum name.
        /// </summary>
        /// <param name="enumName">
        /// Name of the enum.
        /// </param>
        /// <returns>
        /// A qualifier
        /// </returns>
        public static new SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType Parse(string enumName)
        {
            return Map.ParseEnumFromName<SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType>(enumName);
        }
    }
}