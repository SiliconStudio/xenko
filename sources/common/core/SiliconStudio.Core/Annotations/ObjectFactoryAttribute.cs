// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Reflection;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// An attribute that defines a factory class implementing <see cref="IObjectFactory"/>, used to create instances of the related type in design-time scenarios.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ObjectFactoryAttribute : Attribute
    {
        /// <summary>
        /// The type of the factory to use to create instance of the related type.
        /// </summary>
        [NotNull]
        public Type FactoryType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFactoryAttribute"/> class.
        /// </summary>
        /// <param name="factoryType">The factory type that implements <see cref="IObjectFactory"/>.</param>
        public ObjectFactoryAttribute([NotNull] Type factoryType)
        {
            if (factoryType == null) throw new ArgumentNullException(nameof(factoryType));
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            if (!typeof(IObjectFactory).GetTypeInfo().IsAssignableFrom(factoryType.GetTypeInfo())) throw new ArgumentException($@"The given type does not implement {nameof(IObjectFactory)}/", nameof(factoryType));
            if (factoryType.GetTypeInfo().GetConstructor(Type.EmptyTypes) == null) throw new ArgumentException(@"The given type does have a public parameterless constructor.", nameof(factoryType));
#endif
            FactoryType = factoryType;
        }
    }
}
