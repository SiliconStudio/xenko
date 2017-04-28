// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Utilities for input devices
    /// </summary>
    public static class InputDeviceUtils
    {
        /// <summary>
        /// Generates a Guid unique to this name
        /// </summary>
        /// <param name="name">the name to turn into a Guid</param>
        /// <returns>A unique Guid for the given name</returns>
        public static Guid DeviceNameToGuid(string name)
        {
            MemoryStream stream = new MemoryStream();
            DigestStream writer = new DigestStream(stream);
            {
                BinarySerializationWriter serializer = new HashSerializationWriter(writer);
                serializer.Write(typeof(IInputDevice).GetHashCode());
                serializer.Write(name);
            }

            return writer.CurrentHash.ToGuid();
        }
    }
}