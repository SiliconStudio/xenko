// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Windows
{
    /// <summary>
    /// A class representing an thread-safe, process-safe mutex.
    /// </summary>
    public class GlobalMutex : IDisposable
    {
        private FileStream lockFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalMutex"/> class.
        /// </summary>
        /// <param name="mutex">A mutex for which the current thread has ownership.</param>
        private GlobalMutex(FileStream lockFile)
        {
            this.lockFile = lockFile;
        }

        /// <summary>
        /// Releases the mutex.
        /// </summary>
        public void Dispose()
        {
            if (lockFile != null)
            {
                var overlapped = new NativeLockFile.OVERLAPPED();
                NativeLockFile.UnlockFileEx(lockFile.SafeFileHandle, 0, 0, 0, ref overlapped);
                lockFile.Dispose();

                // Try to delete the file
                // Ideally we would use FileOptions.DeleteOnClose, but it doesn't seem to work well with FileShare for second instance
                try
                {
                    File.Delete(lockFile.Name);
                }
                catch (Exception)
                {
                }

                lockFile = null;
            }
        }
        
        /// <summary>
        /// Tries to take ownership of the mutex without waiting.
        /// </summary>
        /// Tries to take ownership of the mutex within a given delay.
        /// <returns>A new instance of <see cref="GlobalMutex"/> if the ownership could be taken, <c>null</c> otherwise.</returns>
        /// <remarks>The returned <see cref="GlobalMutex"/> must be disposed to release the mutex.</remarks>
        public static GlobalMutex TryLock(string name)
        {
            return Wait(name, 0);
        }

        /// <summary>
        /// Waits indefinitely to take ownership of the mutex.
        /// </summary>
        /// Tries to take ownership of the mutex within a given delay.
        /// <returns>A new instance of <see cref="GlobalMutex"/> if the ownership could be taken, <c>null</c> otherwise.</returns>
        /// <remarks>The returned <see cref="GlobalMutex"/> must be disposed to release the mutex.</remarks>
        public static GlobalMutex Wait(string name)
        {
            return Wait(name, -1);
        }

        /// <summary>
        /// Tries to take ownership of the mutex within a given delay.
        /// </summary>
        /// <param name="name">A unique name identifying the global mutex.</param>
        /// <param name="millisecondsTimeout">The maximum delay to wait before returning, in milliseconds.</param>
        /// <returns>A new instance of <see cref="GlobalMutex"/> if the ownership could be taken, <c>null</c> otherwise.</returns>
        /// <remarks>
        /// The returned <see cref="GlobalMutex"/> must be disposed to release the mutex.
        /// Calling this method with 0 for <see paramref="millisecondsTimeout"/> is equivalent to call <see cref="TryLock"/>.
        /// Calling this method with a negative value for <see paramref="millisecondsTimeout"/> is equivalent to call <see cref="Wait(string)"/>.
        /// </remarks>
        public static GlobalMutex Wait(string name, int millisecondsTimeout)
        {
            var mutex = BuildMutex(name);
            try
            {
                if (millisecondsTimeout != 0 && millisecondsTimeout != -1)
                    throw new NotImplementedException("GlobalMutex.Wait() is implemented only for millisecondsTimeout 0 or -1");

                var overlapped = new NativeLockFile.OVERLAPPED();
                bool hasHandle = NativeLockFile.LockFileEx(mutex.SafeFileHandle, NativeLockFile.LOCKFILE_EXCLUSIVE_LOCK | (millisecondsTimeout == 0 ? NativeLockFile.LOCKFILE_FAIL_IMMEDIATELY : 0), 0, uint.MaxValue, uint.MaxValue, ref overlapped);
                return hasHandle == false ? null : new GlobalMutex(mutex);
            }
            catch (AbandonedMutexException)
            {
                return new GlobalMutex(mutex);
            }
        }

        private static FileStream BuildMutex(string name)
        {
            name = name.Replace(":", "_");
            name = name.Replace("/", "_");
            name = name.Replace("\\", "_");
            return new FileStream(name + ".lock", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        }
    }
}
