// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Quantum.View
{
    /// <summary>
    /// An implementation of the <see cref="ObservableNodeTemplateProvider"/> that matches <see cref="IObservableNode"/> of a specific type.
    /// </summary>
    public class TypeMatchTemplateProvider : ObservableNodeTemplateProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMatchTemplateProvider"/> class.
        /// </summary>
        public TypeMatchTemplateProvider()
        {
            AcceptNullable = true;
        }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> to match. This provider will accept any node that has either a <see cref="IObservableNode.Type"/>
        /// or a <see cref="IObservableNode.Value"/> with a type that is assignable to the type represented in this property.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets whether to match nullable instances of the <see cref="Type"/>, when it represents a value type.
        /// </summary>
        public bool AcceptNullable { get; set; }

        /// <inheritdoc/>
        public override string Name { get { return Type.Name; } }

        /// <inheritdoc/>
        public override bool MatchNode(IObservableNode node)
        {
            if (Type == null)
                return true;

            if (MatchType(node, Type))
                return true;

            if (AcceptNullable && Type.IsValueType)
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(new[] { Type });
                return MatchType(node, nullableType);
            }

            return false;
        }
    }
}