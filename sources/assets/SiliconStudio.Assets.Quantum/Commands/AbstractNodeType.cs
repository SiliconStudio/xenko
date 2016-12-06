using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Quantum.Commands
{
    /// <summary>
    /// Represents a type for <see cref="CreateNewInstanceCommand"/>.
    /// </summary>
    public class AbstractNodeType : AbstractNodeEntry
    {
        public AbstractNodeType(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        /// <inheritdoc/>
        public override string DisplayValue => Type.GetDisplayName();

        /// <inheritdoc/>
        public override object GenerateValue(object currentValue) => ObjectFactoryRegistry.NewInstance(Type);

        /// <inheritdoc/>
        public override bool IsMatchingValue(object value) => value?.GetType() == Type;
    }
}