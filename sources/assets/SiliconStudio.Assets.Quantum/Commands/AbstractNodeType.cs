using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
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
        public override object GenerateValue(object currentValue)
        {
            // Check if this type can be created first to avoid exceptions
            if (!ObjectFactoryRegistry.CanCreateInstance(Type))
                return null;

            return ObjectFactoryRegistry.NewInstance(Type);
        }

        /// <inheritdoc/>
        public override bool IsMatchingValue(object value) => value?.GetType() == Type;

        public static IEnumerable<AbstractNodeType> GetInheritedInstantiableTypes(Type type)
        {
            return type.GetInheritedInstantiableTypes().Where(x => Attribute.GetCustomAttribute(x, typeof(NonInstantiableAttribute)) == null).Select(x => new AbstractNodeType(x));
        }

        public override string ToString() => DisplayValue;
    }
}