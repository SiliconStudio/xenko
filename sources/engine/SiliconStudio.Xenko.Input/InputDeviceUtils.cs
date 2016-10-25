// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Input
{
    public static class InputDeviceUtils
    {
        /// <summary>
        /// Generates a Guid unique to this sensor type
        /// </summary>
        /// <param name="sensorName">the name of the sensor</param>
        /// <returns>A unique Guid for the given sensor type</returns>
        public static Guid DeviceNameToGuid(string sensorName)
        {
            MemoryStream stream = new MemoryStream();
            DigestStream writer = new DigestStream(stream);
            {
                BinarySerializationWriter serializer = new HashSerializationWriter(writer);
                serializer.Write(typeof(IInputDevice).GetHashCode());
                serializer.Write(sensorName);
            }
            return writer.CurrentHash.ToGuid();
        }
    }
}