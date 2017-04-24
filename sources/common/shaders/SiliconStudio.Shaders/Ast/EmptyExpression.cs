// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A Empty expression
    /// </summary>
    public partial class EmptyExpression : Expression
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return string.Empty;
        }
    }
}
