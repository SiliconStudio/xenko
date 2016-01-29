// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Defines what generic parameters to pass to the serializer.
    /// </summary>
#if ASSEMBLY_PROCESSOR
    internal enum DataSerializerGenericMode
#else
    public enum DataSerializerGenericMode
#endif
    {
        // ***************************************************************
        // NOTE: This file is shared with the AssemblyProcessor.
        // If this file is modified, the AssemblyProcessor has to be
        // recompiled separately. See build\Xenko-AssemblyProcessor.sln
        // ***************************************************************

        None = 0,
        /// <summary>
        /// The type of the serialized type will be passed as a generic arguments of the serializer.
        /// Example: serializer of A becomes instantiated as Serializer{A}.
        /// </summary>
        Type = 1,
        /// <summary>
        /// The generic arguments of the serialized type will be passed as a generic arguments of the serializer.
        /// Example: serializer of A{T1, T2} becomes instantiated as Serializer{T1, T2}.
        /// </summary>
        GenericArguments = 2,
        /// <summary>
        /// Combinations of both <see cref="Type"/> and <see cref="GenericArguments"/>.
        /// Example: serializer of A{T1, T2} becomes instantiated as Serializer{A, T1, T2}.
        /// </summary>
        TypeAndGenericArguments = 3,
    }
}