using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Custom generic type.
    /// </summary>
    public partial class GenericType : GenericBaseType
    {
        public GenericType()
        {
        }

        public GenericType(string name, int parameterCount) : base(name, parameterCount)
        {
        }

        /// <inheritdoc/>
        [DataMember]
        public override List<Type> ParameterTypes { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public override List<Node> Parameters { get; set; }
    }
}