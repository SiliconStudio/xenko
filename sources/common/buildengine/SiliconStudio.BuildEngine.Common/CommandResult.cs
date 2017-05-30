// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.BuildEngine
{
    [ContentSerializer(typeof(DataContentSerializer<CommandResultEntry>))]
    [DataContract]
    public class CommandResultEntry
    {
        public Dictionary<ObjectUrl, ObjectId> InputDependencyVersions;
        /// <summary>
        /// Output object ids as saved in the object database.
        /// </summary>
        public Dictionary<ObjectUrl, ObjectId> OutputObjects;

        /// <summary>
        /// Log messages corresponding to the execution of the command.
        /// </summary>
        public List<SerializableLogMessage> LogMessages;

        /// <summary>
        /// Tags added for a given URL.
        /// </summary>
        public List<KeyValuePair<ObjectUrl, string>> TagSymbols;

        /// <summary>
        /// Commands created during the execution of the current command.
        /// </summary>
        public List<Command> SpawnedCommands;

        public CommandResultEntry()
        {
            InputDependencyVersions = new Dictionary<ObjectUrl, ObjectId>();
            OutputObjects = new Dictionary<ObjectUrl, ObjectId>();
            LogMessages = new List<SerializableLogMessage>();
            SpawnedCommands = new List<Command>();
            TagSymbols = new List<KeyValuePair<ObjectUrl, string>>();
        }
    }
}
