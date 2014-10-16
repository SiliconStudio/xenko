// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.AssemblyProcessor.Serializers
{
    /// <summary>
    /// Gives the required generic serializer for a given type.
    /// This is useful for generation of serialization assembly, when AOT is performed (all generic serializers must be available).
    /// </summary>
    interface ICecilSerializerProcessor
    {
        /// <summary>
        /// Process serializers for given assembly context.
        /// </summary>
        /// <param name="context">The context.</param>
        void ProcessSerializers(CecilSerializerContext context);
    }
}