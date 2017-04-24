// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A single parameter declaration.
    /// </summary>
    public partial class Parameter : Variable
    {
        private MethodDeclaration declaringMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        public Parameter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        public Parameter(TypeBase type, string name = null, Expression initialValue = null)
            : base(type, name, initialValue)
        {
        }

        #region Public Properties

        /// <summary>
        ///   Gets or sets the declaring method.
        /// </summary>
        /// <value>
        ///   The declaring method.
        /// </value>
        [VisitorIgnore]
        public MethodDeclaration DeclaringMethod
        {
            get
            {
                return declaringMethod;
            }
            set
            {
                declaringMethod = value;
            }
        }

        #endregion
    }
}
