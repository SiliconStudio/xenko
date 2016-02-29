// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Tests.Commands
{
    [ContentSerializer(typeof(DataContentSerializer<DataContainer>))]
    [DataContract]
    [Serializable]
    public class DataContainer 
    {
        public byte[] Data;

        public static DataContainer Load(Stream stream)
        {
            return new DataContainer { Data = Utilities.ReadStream(stream) };
        }

        public DataContainer Alterate()
        {
            var result = new DataContainer { Data = new byte[Data.Length] };
            for (var i = 0; i < Data.Length; ++i)
            {
                unchecked { result.Data[i] = (byte)(Data[i] + 1); }
            }
            return result;
        }
    }

    public sealed class InputOutputTestCommand : IndexFileCommand
    {
        /// <inheritdoc/>
        public override string Title { get { return "InputOutputTestCommand " + Source + " > " + OutputUrl; } }

        public int Delay = 0;

        public Guid Id { get { throw new NotImplementedException(); } }
        public string Location => OutputUrl;

        public ObjectUrl Source;
        public string OutputUrl;
        public List<ObjectUrl> InputDependencies = new List<ObjectUrl>();

        public bool ExecuteRemotely = false;

        public List<Command> CommandsToSpawn = new List<Command>();

        public override string OutputLocation => Location;

        private bool WaitDelay()
        {
            // Simulating actual work on input to generate output
            int nbSleep = Delay / 100;
            for (int i = 0; i < nbSleep; ++i)
            {
                Thread.Sleep(100);
                if (CancellationToken.IsCancellationRequested)
                    return false;
            }

            Thread.Sleep(Delay - (nbSleep * 100));
            return true;
        }

        protected override async Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var assetManager = new ContentManager();
            DataContainer result = null;

            switch (Source.Type)
            {
                case UrlType.File:
                    using (var fileStream = new FileStream(Source.Path, FileMode.Open, FileAccess.Read))
                    {
                        if (!WaitDelay())
                            return ResultStatus.Cancelled;

                        result = DataContainer.Load(fileStream);
                    }
                    break;
                case UrlType.ContentLink:
                case UrlType.Content:
                    var container = assetManager.Load<DataContainer>(Source.Path);

                        if (!WaitDelay())
                            return ResultStatus.Cancelled;

                     result = container.Alterate();
                  break;
            }

            assetManager.Save(OutputUrl, result);

            var tasksToWait = CommandsToSpawn.Select(commandContext.ScheduleAndExecuteCommand);
            await Task.WhenAll(tasksToWait);

            foreach (ObjectUrl inputDep in InputDependencies)
            {
                commandContext.RegisterInputDependency(inputDep);
            }
            return ResultStatus.Successful;
        }

        protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
        {
            yield return Source;
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            writer.Write(Source);
            writer.Write(OutputUrl);
        }

        public override bool ShouldSpawnNewProcess()
        {
            return ExecuteRemotely;
        }

        public override string ToString()
        {
            return "InputOutputTestCommand " + Source + " > " + OutputUrl;
        }

    }
}
