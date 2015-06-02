// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SiliconStudio.Core.Serialization;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using Microsoft.Win32.SafeHandles;
#endif

namespace SiliconStudio.Core.IO
{
    class Store
    {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public uint internalLow;
            public uint internalHigh;
            public uint offsetLow;
            public uint offsetHigh;
            public IntPtr hEvent;
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool LockFileEx(SafeFileHandle handle, uint flags, uint reserved, uint countLow, uint countHigh, ref OVERLAPPED overlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool UnlockFileEx(SafeFileHandle handle, uint reserved, uint countLow, uint countHigh, ref OVERLAPPED overlapped);

        public const uint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;
#endif
    }
    
    /// <summary>
    /// A store that will be incrementally saved on the HDD.
    /// Thread-safe and process-safe.
    /// </summary>
    /// <typeparam name="T">The type of elements in the store.</typeparam>
    public abstract class Store<T> : IDisposable where T : new()
    {
        protected Stream stream;

        protected int transaction;
        protected object lockObject = new object();

        /// <summary>
        /// Gets or sets a flag specifying if the index map changes should be kept aside instead of being committed immediately.
        /// </summary>
        public bool UseTransaction { get; set; }

        /// <summary>
        /// Gets or sets a flag specifying if a Save should also load new values that happened in between.
        /// </summary>
        public bool AutoLoadNewValues { get; set; }

        protected Store(Stream stream)
        {
            AutoLoadNewValues = true;
            this.stream = stream;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Waits for pending operation to finish, if any. Note that it does not write pending transaction if <see cref="Save"/> has not been called before.
        /// </summary>
        public void Dispose()
        {
            if (stream != null)
            {
                lock (stream)
                {
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        /// <summary>
        /// Adds multiple values to the store
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public void AddValues(IEnumerable<T> values)
        {
            lock (lockObject)
            {
                var useTransaction = UseTransaction;
                int currentTransaction = transaction;
                if (!useTransaction)
                    transaction++;

                foreach (var value in values)
                {
                    // Use unsavedIdMap so that loadedIdMap is still coherent before flushed to disk asynchronously (since other processes/threads might write to it as well).
                    AddUnsaved(value, currentTransaction);
                }

                if (!useTransaction)
                {
                    SaveValues(values, currentTransaction);
                }
            }
        }

        /// <summary>
        /// Adds a value to the store.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public void AddValue(T item)
        {
            lock (lockObject)
            {
                var useTransaction = UseTransaction;
                int currentTransaction = transaction;
                if (!useTransaction)
                    transaction++;

                // Use unsavedIdMap so that loadedIdMap is still coherent before flushed to disk asynchronously (since other processes/threads might write to it as well).
                AddUnsaved(item, currentTransaction);

                if (!useTransaction)
                {
                    SaveValue(item, currentTransaction);
                }
            }
        }

        private void SaveValues(IEnumerable<T> values, int currentTransaction)
        {
            if (stream == null)
                throw new InvalidOperationException("No active stream.");

            lock (stream)
            {
                var indexStreamPosition = stream.Position;

                // Acquire lock on end of file (for appending)
                // This will prevent another thread from writing at the same time, or reading before it is flushed.
                LockFile(indexStreamPosition, long.MaxValue, true);

                try
                {
                    // Make sure we read up entries up to end of file (or skip it if AutoLoadNewValues is not set)
                    if (AutoLoadNewValues)
                        RefreshData(stream.Length);
                    else
                        stream.Position = stream.Length;

                    foreach (var value in values)
                    {
                        WriteEntry(stream, value);
                    }
                    stream.Flush();

                    // Transfer from temporary mapping to real mapping (so that loadedIdMap is updated in right order)
                    lock (lockObject)
                    {
                        RemoveUnsaved(values, currentTransaction);
                        foreach (var value in values)
                        {
                            AddLoaded(value);
                        }
                    }
                }
                finally
                {
                    UnlockFile(indexStreamPosition, long.MaxValue);
                }
            }
        }

        private void SaveValue(T item, int currentTransaction)
        {
            if (stream == null)
                throw new InvalidOperationException("No active stream.");

            lock (stream)
            {
                var indexStreamPosition = stream.Position;

                // Acquire lock on end of file (for appending)
                // This will prevent another thread from writing at the same time, or reading before it is flushed.
                LockFile(indexStreamPosition, long.MaxValue, true);

                try
                {
                    // Make sure we read up entries up to end of file (or skip it if AutoLoadNewValues is not set)
                    if (AutoLoadNewValues)
                        RefreshData(stream.Length);
                    else
                        stream.Position = stream.Length;

                    WriteEntry(stream, item);
                    stream.Flush();

                    // Transfer from temporary mapping to real mapping (so that loadedIdMap is updated in right order)
                    lock (lockObject)
                    {
                        RemoveUnsaved(item, currentTransaction);
                        AddLoaded(item);
                    }
                }
                finally
                {
                    UnlockFile(indexStreamPosition, long.MaxValue);
                }
            }
        }

        /// <summary>
        /// Saves the newly added mapping (only necessary when UseTransaction is set to true).
        /// </summary>
        /// <param name="saveNow">if set to <c>true</c> save now the map without using a task.</param>
        public void Save()
        {
            lock (lockObject)
            {
                int currentTransaction = transaction++;
                var transactionIds = GetPendingItems(currentTransaction);

                SaveValues(transactionIds, currentTransaction);


            }
        }

        /// <summary>
        /// Resets the store to an empty state.
        /// </summary>
        public void Reset()
        {
            lock (stream)
            {
                lock (lockObject)
                {
                    stream.Position = 0;
                    stream.SetLength(0);
                    ResetInternal();
                }
            }
        }

        /// <summary>
        /// Resets the store to an empty state, to be implemented by subclasses if necessary.
        /// </summary>
        protected virtual void ResetInternal()
        {
        }

        /// <summary>
        /// Refreshes URL to ObjectId mapping from the latest results in the index file.
        /// </summary>
        /// <returns></returns>
        public bool LoadNewValues()
        {
            if (stream == null)
                throw new InvalidOperationException("No active stream.");

            lock (stream)
            {
                var position = stream.Position;
                var fileSize = stream.Length;

                if (position == fileSize)
                    return true;

                // Lock content that will be read.
                // This lock doesn't prevent concurrent writing since we lock only until current known filesize.
                // In the case where fileSize was taken at the time of an incomplete append, this lock will also wait for completion of the last write.
                // Note: Maybe we should release the lock quickly so that two threads can read at the same time?
                // Or if the previously described case doesn't happen, maybe no lock at all is required?
                // Otherwise, last possibility would be deterministic filesize (with size encoded at the beginning of each block).
                LockFile(position, long.MaxValue, false);

                try
                {
                    // update the size after the lock
                    fileSize = stream.Length;
                    RefreshData(fileSize);
                }
                finally
                {
                    // Release the lock
                    UnlockFile(position, long.MaxValue);
                }

                return true;
            }
        }

        private void LockFile(long offset, long count, bool exclusive)
        {
#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME && !SILICONSTUDIO_PLATFORM_MONO_MOBILE
            var fileStream = stream as FileStream;
            if (fileStream == null)
                return;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            var countLow = (uint)count;
            var countHigh = (uint)(count >> 32);

            var overlapped = new Store.OVERLAPPED()
                {
                    internalLow = 0,
                    internalHigh = 0,
                    offsetLow = (uint)offset,
                    offsetHigh = (uint)(offset >> 32),
                    hEvent = IntPtr.Zero,
                };

            if (!Store.LockFileEx(fileStream.SafeFileHandle, exclusive ? Store.LOCKFILE_EXCLUSIVE_LOCK : 0, 0, countLow, countHigh, ref overlapped))
            {
                throw new IOException("Couldn't lock file.");
            }
#else
            bool tryAgain;
            do
            {
                tryAgain = false;
                try
                {
                    fileStream.Lock(offset, count);
                }
                catch (IOException)
                {
                    tryAgain = true;
                }
            } while (tryAgain);
#endif
#endif
        }

        private void UnlockFile(long offset, long count)
        {
#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME && !SILICONSTUDIO_PLATFORM_MONO_MOBILE
            var fileStream = stream as FileStream;
            if (fileStream == null)
                return;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            var countLow = (uint)count;
            var countHigh = (uint)(count >> 32);

            var overlapped = new Store.OVERLAPPED()
                {
                    internalLow = 0,
                    internalHigh = 0,
                    offsetLow = (uint)offset,
                    offsetHigh = (uint)(offset >> 32),
                    hEvent = IntPtr.Zero,
                };

            if (!Store.UnlockFileEx(fileStream.SafeFileHandle, 0, countLow, countHigh, ref overlapped))
            {
                throw new IOException("Couldn't unlock file.");
            }
#else
            fileStream.Unlock(offset, count);
#endif
#endif
        }

        private void RefreshData(long fileSize)
        {
            var streamBeginPosition = stream.Position;

            // Precache everything in a MemoryStream
            var length = (int)(fileSize - stream.Position);
            if (length == 0)
                return;

            var bufferToRead = new byte[length];
            stream.Read(bufferToRead, 0, length);
            var memoryStream = new MemoryStream(bufferToRead);

            try
            {
                var entries = ReadEntries(memoryStream);

                lock (lockObject)
                {
                    foreach (var entry in entries)
                    {
                        AddLoaded(entry);
                    }
                }
            }
            catch
            {
                // If there was an exception, go back to previous position
                stream.Position = streamBeginPosition;
                throw;
            }
        }

        /// <summary>
        /// Adds a value that has not yet been saved in the store (pending state).
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="transaction">The transaction index.</param>
        protected abstract void AddUnsaved(T item, int transaction);

        /// <summary>
        /// Removes a value that has not yet been saved (pending state).
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="transaction">The transaction index.</param>
        protected abstract void RemoveUnsaved(T item, int transaction);

        /// <summary>
        /// Removes values that have not yet been saved (pending state).
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="transaction">The transaction index.</param>
        protected virtual void RemoveUnsaved(IEnumerable<T> items, int transaction)
        {
            foreach (var item in items)
            {
                RemoveUnsaved(item, transaction);
            }
        }

        /// <summary>
        /// Adds a value that has already been saved in the store (saved state).
        /// </summary>
        /// <param name="item">The item.</param>
        protected abstract void AddLoaded(T item);

        /// <summary>
        /// Gets the list of pending items for a given transaction index.
        /// </summary>
        /// <param name="transaction">The transaction index.</param>
        /// <returns></returns>
        protected abstract IEnumerable<T> GetPendingItems(int transaction);


        protected virtual object BuildContext(Stream stream)
        {
            return stream;
        }

        protected virtual List<T> ReadEntries(Stream localStream)
        {
            var reader = new BinarySerializationReader(localStream);
            var entries = new List<T>();
            while (localStream.Position < localStream.Length)
            {
                var entry = new T();
                reader.Serialize(ref entry, ArchiveMode.Deserialize);
                entries.Add(entry);
            }
            return entries;
        }

        protected virtual void WriteEntry(Stream stream, T value)
        {
            var reader = new BinarySerializationWriter(stream);
            reader.Serialize(ref value, ArchiveMode.Serialize);
        }
    }
}