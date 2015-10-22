// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Ast
{
    /// <summary>
    /// A mixin statement.
    /// </summary>
    public class MixinStatement : Statement, IScopeContainer
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "MixinStatement" /> class.
        /// </summary>
        public MixinStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MixinStatement" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="mixin">The mixin.</param>
        public MixinStatement(MixinStatementType type, Expression mixin)
        {
            Type = type;
            Value = mixin;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public MixinStatementType Type { get; set; }

        /// <summary>
        /// Gets or sets the target of this mixin.
        /// </summary>
        /// <value>The target.</value>
        public Expression Value { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Value);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("mixin {0}{1};", Type > 0 ? Type.ToString().ToLowerInvariant() + " " : string.Empty, Value);
        }

        #endregion
    }
}