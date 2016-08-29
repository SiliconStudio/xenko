// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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