// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Handles <see cref="Guid"/> generation and storage for model nodes.
    /// </summary>
    /// <remarks>This class will hold references on objects until they are unregistered or until the container is cleared.</remarks>
    /// <remarks>This class is thread safe.</remarks>
    public class GuidContainer : IGuidContainer
    {
        private readonly Dictionary<object, Guid> objectGuids = new Dictionary<object, Guid>();

        /// <inheritdoc/>
        public Guid GetOrCreateGuid(object obj)
        {
            if (obj == null) return Guid.NewGuid();

            lock (objectGuids)
            {
                Guid guid;
                if (!objectGuids.TryGetValue(obj, out guid))
                {
                    objectGuids.Add(obj, guid = Guid.NewGuid());
                }
                return guid;
            }
        }

        /// <inheritdoc/>
        public Guid GetGuid(object obj)
        {
            lock (objectGuids)
            {
                Guid guid;
                return obj != null && objectGuids.TryGetValue(obj, out guid) ? guid : Guid.Empty;
            }
        }

        /// <inheritdoc/>
        public void RegisterGuid(Guid guid, object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            lock (objectGuids)
            {
                objectGuids[obj] = guid;
            }
        }

        /// <inheritdoc/>
        public bool UnregisterGuid(Guid guid)
        {
            lock (objectGuids)
            {
                object key = objectGuids.SingleOrDefault(x => x.Value == guid).Key;
                return key != null && objectGuids.Remove(key);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (objectGuids)
            {
                objectGuids.Clear();
            }
        }
    }
}