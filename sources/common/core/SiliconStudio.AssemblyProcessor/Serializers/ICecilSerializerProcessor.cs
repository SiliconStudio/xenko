// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
