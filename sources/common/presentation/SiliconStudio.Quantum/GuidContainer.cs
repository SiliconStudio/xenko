// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Handles <see cref="Guid"/> generation and storage for model nodes.
    /// </summary>
    /// <remarks>This class will hold references on objects until they are unregistered or until the container is cleared.</remarks>
    /// <remarks>This class is thread safe.</remarks>
    public class GuidContainer : IGuidContainer
    {
        private ConditionalWeakTable<object, object> objectGuids = new ConditionalWeakTable<object, object>();

        /// <inheritdoc/>
        public Guid GetOrCreateGuid(object obj)
        {
            if (obj == null) return Guid.NewGuid();

            lock (objectGuids)
            {
                object guid;
                if (!objectGuids.TryGetValue(obj, out guid))
                {
                    objectGuids.Add(obj, guid = Guid.NewGuid());
                }
                return (Guid)guid;
            }
        }

        /// <inheritdoc/>
        public Guid GetGuid(object obj)
        {
            lock (objectGuids)
            {
                object guid;
                return (Guid)(obj != null && objectGuids.TryGetValue(obj, out guid) ? guid : Guid.Empty);
            }
        }

        /// <inheritdoc/>
        public void RegisterGuid(Guid guid, object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            lock (objectGuids)
            {
                objectGuids.Add(obj, guid);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (objectGuids)
            {
                objectGuids = new ConditionalWeakTable<object, object>();
            }
        }
    }
}