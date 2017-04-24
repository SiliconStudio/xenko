// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Shaders.Ast.Glsl
{
    /// <summary>
    /// An interface type.
    /// </summary>
    public partial class InterfaceType : StructType
    {
        public InterfaceType()
        {
        }

        public InterfaceType(string name)
        {
            Name = name;
        }
    }
}
