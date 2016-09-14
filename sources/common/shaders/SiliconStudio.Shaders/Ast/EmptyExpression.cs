// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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