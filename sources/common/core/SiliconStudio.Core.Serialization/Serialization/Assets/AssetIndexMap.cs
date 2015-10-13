// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.IO;
using System.Collections.Generic;
using System.Linq;


namespace SiliconStudio.Core.Serialization.Assets
{
    public sealed class AssetIndexMap : DictionaryStore<string, ObjectId>, IAssetIndexMap
    {
        private static readonly Regex RegexEntry = new Regex(@"^(.*?)\s+(\w+)$");

        private AssetIndexMap()
            : base(null)
        {
        }

        public static AssetIndexMap NewTool(string indexName)
        {
            if (indexName == null) throw new ArgumentNullException(nameof(indexName));

            var result = new AssetIndexMap
            {
                // Try to open with read-write
                stream = VirtualFileSystem.OpenStream(
                    VirtualFileSystem.ApplicationDatabasePath + '/' + indexName,
                    VirtualFileMode.OpenOrCreate,
                    VirtualFileAccess.ReadWrite,
                    VirtualFileShare.ReadWrite)
            };

            return result;
        }

        public static AssetIndexMap CreateInMemory()
        {
            var result = new AssetIndexMap { stream = new MemoryStream() };
            result.LoadNewValues();
            return result;
        }

        public static AssetIndexMap Load(string indexFile, bool isReadOnly = false)
        {
            if (indexFile == null) throw new ArgumentNullException(nameof(indexFile));

            var result = new AssetIndexMap();

            var isAppDataWriteable = !isReadOnly;
            if (isAppDataWriteable)
            {
                try
                {
                    // Try to open with read-write
                    result.stream = VirtualFileSystem.OpenStream(
                        indexFile,
                        VirtualFileMode.OpenOrCreate,
                        VirtualFileAccess.ReadWrite,
                        VirtualFileShare.ReadWrite);
                }
                catch (UnauthorizedAccessException)
                {
                    isAppDataWriteable = false;
                }
            }

            if (!isAppDataWriteable)
            {
                // Try to open read-only
                result.stream = VirtualFileSystem.OpenStream(
                    indexFile,
                    VirtualFileMode.Open,
                    VirtualFileAccess.Read);
            }

            result.LoadNewValues();

            return result;
        }

        public IEnumerable<KeyValuePair<string, ObjectId>> GetTransactionIdMap()
        {
            lock (lockObject)
            {
                return GetPendingItems(transaction);
            }
        }

        public IEnumerable<KeyValuePair<string, ObjectId>> GetMergedIdMap()
        {
            lock (lockObject)
            {
                return unsavedIdMap
                    .Select(x => new KeyValuePair<string, ObjectId>(x.Key, x.Value.Value))
                    .Concat(loadedIdMap.Where(x => !unsavedIdMap.ContainsKey(x.Key)))
                    .ToArray();
            }
        }

        protected override List<KeyValuePair<string, ObjectId>> ReadEntries(Stream localStream)
        {
            var reader = new StreamReader(localStream, Encoding.UTF8);
            string line;
            var entries = new List<KeyValuePair<string, ObjectId>>();
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line == string.Empty || line.StartsWith("#"))
                    continue;

                var match = RegexEntry.Match(line);
                if (!match.Success)
                {
                    throw new InvalidOperationException("Unable to read asset index entry [{0}]. Expecting: [path objectId]".ToFormat(line));
                }

                var url = match.Groups[1].Value;
                var objectIdStr = match.Groups[2].Value;

                ObjectId objectId;
                if (!ObjectId.TryParse(objectIdStr, out objectId))
                {
                    throw new InvalidOperationException("Unable to decode objectid [{0}] when reading asset index".ToFormat(objectIdStr));
                }

                var entry =  new KeyValuePair<string, ObjectId>(url, objectId);
                entries.Add(entry);
            }
            return entries;
        }

        protected override void WriteEntry(Stream stream, KeyValuePair<string, ObjectId> value)
        {
            var line = $"{value.Key} {value.Value}\n";
            var bytes = Encoding.UTF8.GetBytes(line);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
