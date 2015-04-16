// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.BuildEngine
{
    internal class BuildTransaction
    {
        private readonly Dictionary<ObjectUrl, ObjectId> transactionOutputObjects = new Dictionary<ObjectUrl, ObjectId>();
        private readonly IAssetIndexMap assetIndexMap;
        private readonly IEnumerable<IDictionary<ObjectUrl, OutputObject>> outputObjectsGroups;

        public BuildTransaction(IAssetIndexMap assetIndexMap, IEnumerable<IDictionary<ObjectUrl, OutputObject>> outputObjectsGroups)
        {
            this.assetIndexMap = assetIndexMap;
            this.outputObjectsGroups = outputObjectsGroups;
        }

        public IEnumerable<KeyValuePair<ObjectUrl, ObjectId>> GetTransactionIdMap()
        {
            return transactionOutputObjects;
        }

        public IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(Func<KeyValuePair<string, ObjectId>, bool> predicate)
        {
            lock (transactionOutputObjects)
            {
                return transactionOutputObjects.Select(x => new KeyValuePair<string, ObjectId>(x.Key.Path, x.Value)).Where(predicate).ToList();
            }
        }

        public bool TryGetValue(string url, out ObjectId objectId)
        {
            var objUrl = new ObjectUrl(UrlType.Internal, url);

            // Lock TransactionAssetIndexMap
            lock (transactionOutputObjects)
            {
                if (transactionOutputObjects.TryGetValue(objUrl, out objectId))
                    return true;

                foreach (var outputObjects in outputObjectsGroups)
                {
                    // Lock underlying EnumerableBuildStep.OutputObjects
                    lock (outputObjects)
                    {
                        OutputObject outputObject;
                        if (outputObjects.TryGetValue(objUrl, out outputObject))
                        {
                            objectId = outputObject.ObjectId;
                            return true;
                        }
                    }
                }

                // Check asset index map (if set)
                if (assetIndexMap != null)
                {
                    if (assetIndexMap.TryGetValue(url, out objectId))
                        return true;
                }
            }

            objectId = ObjectId.Empty;
            return false;
        }

        internal class DatabaseAssetIndexMap : IAssetIndexMap
        {
            private readonly BuildTransaction buildTransaction;

            public DatabaseAssetIndexMap(BuildTransaction buildTransaction)
            {
                this.buildTransaction = buildTransaction;
            }

            public bool TryGetValue(string url, out ObjectId objectId)
            {
                return buildTransaction.TryGetValue(url, out objectId);
            }

            public bool Contains(string url)
            {
                ObjectId objectId;
                return TryGetValue(url, out objectId);
            }

            public ObjectId this[string url]
            {
                get
                {
                    ObjectId objectId;
                    if (!TryGetValue(url, out objectId))
                        throw new KeyNotFoundException();

                    return objectId;
                }
                set
                {
                    lock (buildTransaction.transactionOutputObjects)
                    {
                        buildTransaction.transactionOutputObjects[new ObjectUrl(UrlType.Internal, url)] = value;
                    }
                }
            }

            public IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(Func<KeyValuePair<string, ObjectId>, bool> predicate)
            {
                return buildTransaction.SearchValues(predicate);
            }

            public void WaitPendingOperations()
            {
            }

            public IEnumerable<KeyValuePair<string, ObjectId>> GetMergedIdMap()
            {
                // Shouldn't be used
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }
        }
    }
}