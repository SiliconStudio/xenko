// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    /// <summary>
    /// Shader Class.
    /// </summary>
    public partial class ShaderClassType : ClassType
    {
        // temp
        public List<Variable> ShaderGenerics { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderClassType"/> class.
        /// </summary>
        public ShaderClassType()
        {
            ShaderGenerics = new List<Variable>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderClassType"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ShaderClassType(string name) : base(name)
        {
            ShaderGenerics = new List<Variable>();
        }

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.AddRange(BaseClasses);
            ChildrenList.AddRange(GenericParameters);
            ChildrenList.AddRange(ShaderGenerics);
            ChildrenList.AddRange(Members);
            return ChildrenList;
        }
    }
}
