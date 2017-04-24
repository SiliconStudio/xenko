// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// A specialization of the <see cref="SerializableLogMessage"/> class that contains a timestamp information.
    /// </summary>
    [DataContract]
    public class SerializableTimestampLogMessage : SerializableLogMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableTimestampLogMessage"/> class with default values for its properties
        /// </summary>
        public SerializableTimestampLogMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableTimestampLogMessage"/> class from a <see cref="TimestampLocalLogger.Message"/> instance.
        /// </summary>
        /// <param name="message">The <see cref="TimestampLocalLogger.Message"/> instance to use to initialize properties.</param>
        public SerializableTimestampLogMessage(TimestampLocalLogger.Message message)
            : base((LogMessage)message.LogMessage)
        {
            Timestamp = message.Timestamp;
        }

        /// <summary>
        /// Gets or sets the timestamp of this message.
        /// </summary>
        public long Timestamp { get; set; }
    }
}
