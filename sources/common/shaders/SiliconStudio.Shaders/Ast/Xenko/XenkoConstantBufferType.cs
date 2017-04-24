// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Shaders.Ast.Hlsl;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class XenkoConstantBufferType : SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType
    {
        /// <summary>
        ///   Resource group keyword (rgroup).
        /// </summary>
        public static readonly XenkoConstantBufferType ResourceGroup = new XenkoConstantBufferType("rgroup");

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
            if (enumName == (string)ResourceGroup.Key)
                return ResourceGroup;

            return ConstantBufferType.Parse(enumName);
        }
    }
}
