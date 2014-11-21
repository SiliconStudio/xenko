// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// A <see cref="Command"/> that reads and/or writes to the index file.
    /// </summary>
    public abstract class IndexFileCommand : Command
    {
        private BuildTransaction buildTransaction;
        internal static ObjectDatabase ObjectDatabase;

        public static MicroThreadLocal<DatabaseFileProvider> DatabaseFileProvider = new MicroThreadLocal<DatabaseFileProvider>(() =>
            {
                throw new InvalidOperationException("No VirtualFileProvider set for this microthread.");
            });

        private static readonly IDictionary<ObjectUrl, OutputObject> commomOutputObjects = new Dictionary<ObjectUrl, OutputObject>();

        public static void MergeOutputObjects(IDictionary<ObjectUrl, OutputObject>  outputObjects)
        {
            lock (commomOutputObjects)
            {
                foreach (var outputObject in outputObjects)
                    commomOutputObjects[outputObject.Key] = outputObject.Value;
            }
        }

        /// <summary>
        /// Creates and mounts a database containing the given output object groups and the common group in the microthread-local <see cref="DatabaseFileProvider"/>.
        /// </summary>
        /// <param name="outputObjectsGroups">A collection of dictionaries representing a group of output object.</param>
        public static void MountDatabase(IEnumerable<IDictionary<ObjectUrl, OutputObject>> outputObjectsGroups)
        {
            MountDatabases(CreateTransaction(outputObjectsGroups));
        }
        
        private static IEnumerable<IDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups(IEnumerable<IDictionary<ObjectUrl, OutputObject>> transactionOutputObjectsGroups)
        {
            if (transactionOutputObjectsGroups != null)
            {
                foreach (var outputObjects in transactionOutputObjectsGroups)
                    yield return outputObjects;
            }
            yield return commomOutputObjects;
        }

        private static void MountDatabases(BuildTransaction transaction)
        {
            DatabaseFileProvider.Value = CreateDatabases(transaction); 
        }

        private static BuildTransaction CreateTransaction(IEnumerable<IDictionary<ObjectUrl, OutputObject>> transactionOutputObjectsGroups)
        {
            return new BuildTransaction(GetOutputObjectsGroups(transactionOutputObjectsGroups));
        }

        private static DatabaseFileProvider CreateDatabases(BuildTransaction transaction)
        {
            return new DatabaseFileProvider(new BuildTransaction.DatabaseAssetIndexMap(transaction), ObjectDatabase);
        }

        public static DatabaseFileProvider GetCommonDatabase()
        {
            return CreateDatabases(CreateTransaction(null));
        }

        internal static void UnmountDatabase()
        {
            DatabaseFileProvider.Value = null;
        }

        public override void PreCommand(ICommandContext commandContext)
        {
            base.PreCommand(commandContext);

            buildTransaction = CreateTransaction(commandContext.GetOutputObjectsGroups());
            MountDatabases(buildTransaction);
        }

        public override void PostCommand(ICommandContext commandContext, ResultStatus status)
        {
            base.PostCommand(commandContext, status);

            if (status == ResultStatus.Successful)
            {
                // Save list of newly changed URLs in CommandResult.OutputObjects
                foreach (var entry in buildTransaction.GetTransactionIdMap())
                {
                    commandContext.RegisterOutput(entry.Key, entry.Value);
                }

                // Note: In case of remote process, the remote process will save the index map.
                // Alternative would be to not save it and just forward results to the master builder who would commit results locally.
                // Not sure which is the best.
                //
                // Anyway, current approach should be OK for now since the index map is "process-safe" (as long as we load new values as necessary).
                //AssetIndexMap.Save();
            }

            DatabaseFileProvider.Value = null;
        }
    }
}