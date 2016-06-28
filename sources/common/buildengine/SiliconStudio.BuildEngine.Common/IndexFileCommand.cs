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
        private static readonly IDictionary<ObjectUrl, OutputObject> CommonOutputObjects = new Dictionary<ObjectUrl, OutputObject>();
        private static readonly MicroThreadLocal<DatabaseFileProvider> MicroThreadLocalDatabaseFileProvider;
        public static ObjectDatabase ObjectDatabase;

        static IndexFileCommand()
        {
            MicroThreadLocalDatabaseFileProvider = new MicroThreadLocal<DatabaseFileProvider>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a valid database file provider.
        /// </summary>
        /// <value><c>true</c> if this instance has database file provider; otherwise, <c>false</c>.</value>
        public static bool HasValidDatabaseFileProvider => MicroThreadLocalDatabaseFileProvider.Value != null;

        /// <summary>
        /// Gets the currently mounted microthread-local database provider.
        /// </summary>
        public static DatabaseFileProvider DatabaseFileProvider => MicroThreadLocalDatabaseFileProvider.Value;

        /// <summary>
        /// Merges the given dictionary of build output objects into the common group. Objects merged here will be integrated to every database,
        /// </summary>
        /// <param name="outputObjects">The dictionary containing the <see cref="OutputObject"/> to merge into the common group.</param>
        public static void MergeOutputObjectsInCommonGroup(IDictionary<ObjectUrl, OutputObject>  outputObjects)
        {
            lock (CommonOutputObjects)
            {
                foreach (var outputObject in outputObjects)
                    CommonOutputObjects[outputObject.Key] = outputObject.Value;
            }
        }

        /// <summary>
        /// Gets a <see cref="MicroThreadLocalDatabaseFileProvider"/> containing only objects from the common group. The common group is a group of objects registered
        /// via <see cref="MergeOutputObjectsInCommonGroup"/> and shared amongst all databases.
        /// </summary>
        /// <returns>A <see cref="MicroThreadLocalDatabaseFileProvider"/> that can provide objects from the common group.</returns>
        public static DatabaseFileProvider GetCommonDatabase()
        {
            return CreateDatabase(CreateTransaction(null));
        }
        
        /// <summary>
        /// Creates and mounts a database containing the given output object groups and the common group in the microthread-local <see cref="MicroThreadLocalDatabaseFileProvider"/>.
        /// </summary>
        /// <param name="outputObjectsGroups">A collection of dictionaries representing a group of output object.</param>
        public static void MountDatabase(IEnumerable<IDictionary<ObjectUrl, OutputObject>> outputObjectsGroups)
        {
            MountDatabase(CreateTransaction(outputObjectsGroups));
        }

        /// <summary>
        /// Creates and mounts a database containing output objects merged in the common group via <see cref="MergeOutputObjectsInCommonGroup"/>
        /// in the microthread-local <see cref="MicroThreadLocalDatabaseFileProvider"/>.
        /// </summary>
        public static void MountCommonDatabase()
        {
            MicroThreadLocalDatabaseFileProvider.Value = CreateDatabase(CreateTransaction(null));
        }

        /// <summary>
        /// Unmount the currently mounted microthread-local database.
        /// </summary>
        public static void UnmountDatabase()
        {
            MicroThreadLocalDatabaseFileProvider.ClearValue();
        }
        
        private static IEnumerable<IDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups(IEnumerable<IDictionary<ObjectUrl, OutputObject>> transactionOutputObjectsGroups)
        {
            if (transactionOutputObjectsGroups != null)
            {
                foreach (var outputObjects in transactionOutputObjectsGroups)
                    yield return outputObjects;
            }
            yield return CommonOutputObjects;
        }


        private static void MountDatabase(BuildTransaction transaction)
        {
            MicroThreadLocalDatabaseFileProvider.Value = CreateDatabase(transaction); 
        }

        private static BuildTransaction CreateTransaction(IEnumerable<IDictionary<ObjectUrl, OutputObject>> transactionOutputObjectsGroups)
        {
            return new BuildTransaction(ObjectDatabase.AssetIndexMap, GetOutputObjectsGroups(transactionOutputObjectsGroups));
        }

        private static DatabaseFileProvider CreateDatabase(BuildTransaction transaction)
        {
            return new DatabaseFileProvider(new BuildTransaction.DatabaseAssetIndexMap(transaction), ObjectDatabase);
        }

        public override void PreCommand(ICommandContext commandContext)
        {
            base.PreCommand(commandContext);

            buildTransaction = CreateTransaction(commandContext.GetOutputObjectsGroups());
            MountDatabase(buildTransaction);
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

            MicroThreadLocalDatabaseFileProvider.ClearValue();
        }
    }
}