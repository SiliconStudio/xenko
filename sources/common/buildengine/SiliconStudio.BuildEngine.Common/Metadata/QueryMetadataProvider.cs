// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.BuildEngine
{
    // TODO: sanitize SQL queries, ensure keys are valid and no forbidden char are passed
    public class QueryMetadataProvider : IMetadataProvider
    {
        public const string DefaultDatabaseFilename = "Metadata.db";

        private readonly Dictionary<string, long> objectUrlIds = new Dictionary<string, long>();
        private readonly Dictionary<MetadataKey, long> keyIds = new Dictionary<MetadataKey, long>();

        private SQLiteConnection connection;

        private readonly object lockObject = new object();

        public bool Open(string path, bool create)
        {
            if (!File.Exists(path))
            {
                return create && Create(path);
            }

            string connectionString = String.Format("Data Source={0}", path);
            lock (lockObject)
            {
                // Connection already opened
                if (connection != null)
                    throw new InvalidOperationException("Connection to the metadata database already opened");

                connection = new SQLiteConnection(connectionString);
                // TODO: check for exception, set connection to null in case of failure
                connection.Open();
                return true;
            }
        }

        public Task<bool> OpenAsync(string path, bool create)
        {
            return Task.Run(() => Open(path, create));
        }

        /// <summary>
        /// Create and open
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Create(string path)
        {
            try
            {
                SQLiteConnection.CreateFile(path);
            }
            catch (IOException)
            {
                return false;
            }

            if (!Open(path, false))
                return false;

            const string Query = @"
                CREATE TABLE `Keys` (
                    `KeyId` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    `TypeId` INTEGER NOT NULL,
                    `Name` char(255) NOT NULL
                );
                CREATE TABLE `ObjectUrls` (
                    `ObjectUrlId` INTEGER NOT NULL PRIMARY KEY,
                    `Location` char(255) NOT NULL
                );
                CREATE TABLE `Metadata` (
                    `ObjectUrlId` INTEGER NOT NULL,
                    `KeyId` INTEGER NOT NULL,
                    `Value`,
                    PRIMARY KEY (`ObjectUrlId`, `KeyId`),
                    FOREIGN KEY(ObjectUrlId) REFERENCES ObjectUrls(ObjectUrlId),
                    FOREIGN KEY(KeyId) REFERENCES Keys(KeyId)
                );
                ";

            ExecuteNonQuery(Query);
            return true;
        }

        public Task<bool> CreateAsync(string path)
        {
            return Task.Run(() => Create(path));
        }


        public void Close()
        {
            lock (lockObject)
            {
                if (connection != null)
                    connection.Close();

                connection = null;
            }
        }

        public Task CloseAsync()
        {
            return Task.Run(() => Close());
        }

        public IEnumerable<MetadataKey> FetchAllKeys()
        {
            var keysToRemove = new List<MetadataKey>(keyIds.Keys);
            const string Query = @"SELECT * FROM `Keys`";

            DataTable dataTable = ExecuteReader(Query);
            foreach (DataRow row in dataTable.Rows)
            {
                var typeId = (int)(long)row["TypeId"];
                var keyId = (long)row["KeyId"];
                if (typeId >= 0 && typeId < Enum.GetValues(typeof(MetadataKey.DatabaseType)).Length)
                {
                    var key = new MetadataKey((string)row["Name"], (MetadataKey.DatabaseType)typeId);
                    keyIds[key] = keyId;
                    keysToRemove.Remove(key);
                }
            }
            // Also remove keys that has been removed
            foreach (var key in keysToRemove)
            {
                keyIds.Remove(key);
            }
            return keyIds.Keys.ToArray();
        }

        public Task<IEnumerable<MetadataKey>> FetchAllKeysAsync()
        {
            return Task.Run(() => FetchAllKeys());
        }

        public IEnumerable<string> FetchAllObjectUrls()
        {
            var urlsToRemove = new List<string>();
            const string Query = @"SELECT * FROM `ObjectUrls`";
            DataTable dataTable = ExecuteReader(Query);
            foreach (DataRow row in dataTable.Rows)
            {
                var url = (string)row["Location"];
                var urlId = (long)row["ObjectUrlId"];
                objectUrlIds[url] = urlId;
                urlsToRemove.SwapRemove(url);
            }
            // Also remove keys that has been removed
            foreach (var url in urlsToRemove)
            {
                objectUrlIds.Remove(url);
            }
            return objectUrlIds.Keys.ToArray();
        }

        public Task<IEnumerable<string>> FetchAllObjectUrlsAsync()
        {
            return Task.Run(() => FetchAllObjectUrls());
        }

        public IEnumerable<IObjectMetadata> Fetch(string objectUrl)
        {
            string query = String.Format(@"SELECT * FROM `Metadata` INNER JOIN ObjectUrls ON `ObjectUrls`.`ObjectUrlId` = `Metadata`.`ObjectUrlId` INNER JOIN Keys ON `Keys`.`KeyId` = `Metadata`.`KeyId` WHERE `ObjectUrls`.`Location` = '{0}'", FormatUrl(objectUrl));
            return ParseResult(ExecuteReader(query));
        }

        public Task<IEnumerable<IObjectMetadata>> FetchAsync(string objectUrl)
        {
            return Task.Run(() => Fetch(objectUrl));
        }

        public IEnumerable<IObjectMetadata> Fetch(MetadataKey key)
        {
            string query = String.Format(@"SELECT * FROM `Metadata` INNER JOIN ObjectUrls ON `ObjectUrls`.`ObjectUrlId` = `Metadata`.`ObjectUrlId` INNER JOIN Keys ON `Keys`.`KeyId` = `Metadata`.`KeyId` WHERE `Keys`.`Name` = '{0}' AND `Keys`.`TypeId` = '{1}'", key.Name, (int)key.Type);
            return ParseResult(ExecuteReader(query));
        }

        public Task<IEnumerable<IObjectMetadata>> FetchAsync(MetadataKey key)
        {
            return Task.Run(() => Fetch(key));
        }

        public IObjectMetadata Fetch(string objectUrl, MetadataKey key)
        {
            string query = String.Format(@"SELECT * FROM `Metadata` INNER JOIN ObjectUrls ON `ObjectUrls`.`ObjectUrlId` = `Metadata`.`ObjectUrlId` INNER JOIN Keys ON `Keys`.`KeyId` = `Metadata`.`KeyId` WHERE `ObjectUrls`.`Location` = '{0}' AND `Keys`.`Name` = '{1}' AND `Keys`.`TypeId` = '{2}'", FormatUrl(objectUrl), key.Name, (int)key.Type);
            return ParseResult(ExecuteReader(query)).SingleOrDefault();
        }

        public Task<IObjectMetadata> FetchAsync(string objectUrl, MetadataKey key)
        {
            return Task.Run(() => Fetch(objectUrl, key));
        }

        public IObjectMetadata Fetch(IObjectMetadata data)
        {
            return Fetch(data.ObjectUrl, data.Key);
        }

        public Task<IObjectMetadata> FetchAsync(IObjectMetadata data)
        {
            return Task.Run(() => Fetch(data.ObjectUrl, data.Key));
        }
        
        public IEnumerable<IObjectMetadata> FetchAll()
        {
            string query = String.Format(@"SELECT * FROM `Metadata` INNER JOIN ObjectUrls ON `ObjectUrls`.`ObjectUrlId` = `Metadata`.`ObjectUrlId` INNER JOIN Keys ON `Keys`.`KeyId` = `Metadata`.`KeyId`");

            return ParseResult(ExecuteReader(query));
        }

        public Task<IEnumerable<IObjectMetadata>> FetchAllAsync()
        {
            return Task.Run(() => FetchAll());
        }

        private IEnumerable<IObjectMetadata> ParseResult(DataTable dataTable)
        {
            var result = new List<IObjectMetadata>();
            foreach (DataRow row in dataTable.Rows)
            {
                var url = (string)row["Location"];
                var name = (string)row["Name"];
                var type = (MetadataKey.DatabaseType)(long)row["TypeId"];
                var key = new MetadataKey(name, type);
                var keyId = (long)row["KeyId"];
                var objectUrlId = (long)row["ObjectUrlId"];
                keyIds[key] = keyId;
                objectUrlIds[FormatUrl(url)] = objectUrlId;
                object value = key.ConvertValue(row["Value"].ToString());
                result.Add(new ObjectMetadata(url, key, value));
            }
            return result;
        }

        public bool AddKey(MetadataKey key)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (!key.IsValid()) throw new ArgumentException(@"Key is invalid.", "key");
            var typeId = (int)key.Type;
            if (typeId != -1)
            {
                lock (lockObject)
                {
                    if (connection != null)
                    {
                        // TODO/Benlitz: a transaction that first try to fetch the key. If it exists, it should return false
                        string query = String.Format(@"INSERT INTO `Keys` (`TypeId`, `Name`) VALUES ('{0}', '{1}')", typeId, key.Name);
                        return ExecuteNonQuery(query) == 1;
                    }
                }
            }
            return false;
        }

        public bool RemoveKey(MetadataKey key)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (!key.IsValid()) throw new ArgumentException(@"Key is invalid.", "key");
            var typeId = (int)key.Type;
            if (typeId != -1)
            {
                lock (lockObject)
                {
                    if (connection != null)
                    {
                        string query = String.Format(@"DELETE FROM `Keys` WHERE `Keys`.`TypeId` = '{0}' AND `Keys`.`Name` = '{1}'", typeId, key.Name);
                        return ExecuteNonQuery(query) == 1;
                    }
                }
            }
            return false;
        }

        public bool Write(IObjectMetadata data)
        {
            if (data == null) throw new ArgumentNullException("data");

            long keyId, objectUrlId;
            keyIds.TryGetValue(data.Key, out keyId);
            objectUrlIds.TryGetValue(FormatUrl(data.ObjectUrl), out objectUrlId);

            IObjectMetadata previousData;
            if (keyId != 0 && objectUrlId != 0)
            {
                previousData = Fetch(objectUrlId, keyId);
            }
            else
            {
                previousData = Fetch(data);
            }

            // Insert
            if (previousData == null)
            {
                keyId = keyId == 0 ? GetKeyId(data.Key) : keyId;
                objectUrlId = objectUrlId == 0 ? GetOrCreateObjectUrlId(data.ObjectUrl) : objectUrlId;

                if (keyId == 0)
                    throw new InvalidOperationException(String.Format("The key {0} does not exist in database.", data.Key));

                return InsertMetadata(objectUrlId, keyId, data.Value.ToString());
            }

            // Update
            return UpdateMetadata(objectUrlId, keyId, data.Value.ToString());
        }

        public bool Delete(IObjectMetadata data)
        {
            long keyId, objectUrlId;
            keyIds.TryGetValue(data.Key, out keyId);
            objectUrlIds.TryGetValue(FormatUrl(data.ObjectUrl), out objectUrlId);
            keyId = keyId == 0 ? GetKeyId(data.Key) : keyId;
            objectUrlId = objectUrlId == 0 ? GetOrCreateObjectUrlId(data.ObjectUrl) : objectUrlId;

            string query = String.Format(@"DELETE FROM `Metadata` WHERE `Metadata`.`ObjectUrlId` = '{0}' AND `Metadata`.`KeyId` = '{1}'", objectUrlId, keyId);
            return ExecuteNonQuery(query) == 1;
        }

        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Escape single quotes and convert the string to lower invariant.
        /// </summary>
        /// <param name="url">The url to format.</param>
        /// <returns>The url formatted accordingly.</returns>
        private static string FormatUrl(string url)
        {
            return url.Replace("'", "''").ToLowerInvariant();
        }

        private long GetKeyId(MetadataKey key)
        {
            if (key == null) throw new ArgumentNullException("key");
            string query = String.Format(@"SELECT `KeyId` FROM `Keys` WHERE `Name` = '{0}' AND `TypeId` = '{1}'", key.Name, (int)key.Type);
            var result = ExecuteScalar(query);
            if (result != null)
                keyIds[key] = (long)result;
            return result != null ? (long)result : 0;
        }

        private long GetObjectUrlId(string url)
        {
            if (url == null) throw new ArgumentNullException("url");
            string query = String.Format(@"SELECT `ObjectUrlId` FROM `ObjectUrls` WHERE `Location` = '{0}'", FormatUrl(url));
            var result = ExecuteScalar(query);
            if (result != null)
                objectUrlIds[FormatUrl(url)] = (long)result;
            return result != null ? (long)result : 0;
        }

        private bool CreateObjectUrlId(string url)
        {
            if (url == null) throw new ArgumentNullException("url");
            string query = String.Format(@"INSERT INTO `ObjectUrls` (`Location`) VALUES ('{0}')", FormatUrl(url));
            return ExecuteNonQuery(query) == 1;
        }

        private long GetOrCreateObjectUrlId(string url)
        {
            if (url == null) throw new ArgumentNullException("url");
            long objectUrlId = GetObjectUrlId(url);
            if (objectUrlId == 0)
            {
                if (CreateObjectUrlId(url))
                    objectUrlId = GetObjectUrlId(url);
            }
            return objectUrlId;
        }

        private IObjectMetadata Fetch(long objectUrlId, long keyId)
        {
            string query = String.Format(@"SELECT * FROM `Metadata` INNER JOIN ObjectUrls ON `ObjectUrls`.`ObjectUrlId` = `Metadata`.`ObjectUrlId` INNER JOIN Keys ON `Keys`.`KeyId` = `Metadata`.`KeyId` WHERE `Metadata`.`ObjectUrlId` = '{0}' AND `Metadata`.`KeyId` = '{1}'", objectUrlId, keyId);
            return ParseResult(ExecuteReader(query)).SingleOrDefault();
        }

        private bool InsertMetadata(long objectUrlId, long keyId, string value)
        {
            string query = String.Format(@"INSERT INTO `Metadata` (`ObjectUrlId`, `KeyId`, `Value`) VALUES ('{0}', '{1}', '{2}')", objectUrlId, keyId, value);
            return ExecuteNonQuery(query) == 1;
        }

        private bool UpdateMetadata(long objectUrlId, long keyId, string value)
        {
            string query = String.Format(@"UPDATE `Metadata` SET `Value` = '{0}' WHERE `ObjectUrlId` = '{1}' AND `KeyId` = '{2}'", value, objectUrlId, keyId);
            return ExecuteNonQuery(query) == 1;
        }

        private int ExecuteNonQuery(string query)
        {
            lock (lockObject)
            {
                var command = new SQLiteCommand(connection) { CommandText = query };
                return command.ExecuteNonQuery();
            }
        }

        private object ExecuteScalar(string query)
        {
            lock (lockObject)
            {
                var command = new SQLiteCommand(connection) { CommandText = query };
                return command.ExecuteScalar();
            }
        }

        private DataTable ExecuteReader(string query)
        {
            lock (lockObject)
            {
                var command = new SQLiteCommand(connection) { CommandText = query };
                SQLiteDataReader reader = command.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(reader);
                return dataTable;
            }
        }
    }
}