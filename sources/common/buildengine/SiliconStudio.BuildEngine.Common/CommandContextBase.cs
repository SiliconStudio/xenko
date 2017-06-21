// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Storage;
using System.Threading.Tasks;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.BuildEngine
{
    public abstract class CommandContextBase : ICommandContext
    {
        public Command CurrentCommand { get; }

        public abstract LoggerResult Logger { get; }

        public BuildParameterCollection BuildParameters { get; }

        protected internal readonly CommandResultEntry ResultEntry;

        public abstract IEnumerable<IDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups();

        protected internal abstract ObjectId ComputeInputHash(UrlType type, string filePath);

        protected CommandContextBase(Command command, BuilderContext builderContext)
        {
            CurrentCommand = command;
            BuildParameters = builderContext.Parameters;
            ResultEntry = new CommandResultEntry();
        }

        public void RegisterInputDependency(ObjectUrl url)
        {
            ResultEntry.InputDependencyVersions.Add(url, ComputeInputHash(url.Type, url.Path));
        }

        public void RegisterOutput(ObjectUrl url, ObjectId hash)
        {
            ResultEntry.OutputObjects.Add(url, hash);
        }

        public void RegisterCommandLog(IEnumerable<ILogMessage> logMessages)
        {
            foreach (var message in logMessages)
            {
                ResultEntry.LogMessages.Add(message as SerializableLogMessage ?? new SerializableLogMessage((LogMessage)message));
            }
        }

        public void AddTag(ObjectUrl url, TagSymbol tagSymbol)
        {
            ResultEntry.TagSymbols.Add(new KeyValuePair<ObjectUrl, string>(url, tagSymbol.Name));
        }
    }
}
