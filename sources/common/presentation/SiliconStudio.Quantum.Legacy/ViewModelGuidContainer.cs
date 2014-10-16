// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Quantum.Legacy
{
    /// <summary>
    /// Handles <see cref="Guid"/> generation for object in the view model system.
    /// This allows sharing object references between multiple <see cref="ViewModelContainer"/>.
    /// </summary>
    public class ViewModelGuidContainer
    {
        // TODO: Use weak references? This would allow to remove the need for Clean()
        private readonly Dictionary<object, Guid> objectGuids = new Dictionary<object, Guid>(new ObjectEqualityComparer());

        /// <summary>
        /// Gets or or create a <see cref="Guid"/> for a given object. If the object is <c>null</c>, a new Guid will be returned.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The <see cref="Guid"/> associated to the given object, or a newly registered <see cref="Guid"/> if the object was not previously registered.</returns>
        internal Guid GetOrCreateGuid(object obj)
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

        /// <summary>
        /// Gets the <see cref="Guid"/> for a given object, if available.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The <see cref="Guid"/> associated to the given object, or <see cref="Guid.Empty"/> if the object was not previously registered.</returns>
        internal Guid GetGuid(object obj)
        {
            lock (objectGuids)
            {
                Guid guid;
                return obj != null && objectGuids.TryGetValue(obj, out guid) ? guid : Guid.Empty;
            }
        }

        /// <summary>
        /// Register the given <see cref="Guid"/> to the given object. If a <see cref="Guid"/> is already associated to this object, it is replaced by the new one.
        /// </summary>
        /// <param name="guid">The guid to register.</param>
        /// <param name="obj">The object to register.</param>
        internal void RegisterGuid(Guid guid, object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            lock (objectGuids)
            {
                objectGuids[obj] = guid;
            }
        }

        /// <summary>
        /// Remove every registered Guid that is not in the passed <see cref="IEnumerable{ViewModelContext}"/>.
        /// It should be called frequently to avoid memory leaks.
        /// </summary>
        /// <param name="viewModelContexts">The enumeration of currently used instances of <see cref="ViewModelContext"/>.</param>
        public void Clean(IEnumerable<ViewModelContext> viewModelContexts)
        {
            if (viewModelContexts == null) throw new ArgumentNullException("viewModelContexts");

            var activeObjects = new HashSet<Guid>();
            foreach (var viewModelByGuid in viewModelContexts.SelectMany(x => x.ViewModelByGuid))
            {
                activeObjects.Add(viewModelByGuid.Value.Guid);
            }

            lock (objectGuids)
            {
                foreach (var item in objectGuids.Where(x => !activeObjects.Contains(x.Value)).ToArray())
                {
                    objectGuids.Remove(item.Key);
                }
            }
        }

        // TODO: Not obsolete but need to be updated
        private class ObjectEqualityComparer : EqualityComparer<object>
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            public override bool Equals(object x, object y)
            // ReSharper restore MemberHidesStaticFromOuterClass
            {
                if (x is ViewModelReference && y is ViewModelReference)
                {
                    return ((ViewModelReference)x).Guid == ((ViewModelReference)y).Guid;
                }
                if (x is IList<ViewModelReference> && y is IList<ViewModelReference>)
                {
                    return ArrayExtensions.ArraysEqual((IList<ViewModelReference>)x, (IList<ViewModelReference>)y);
                }
                return object.Equals(x, y);
            }

            public override int GetHashCode(object obj)
            {
                if (obj == null) throw new ArgumentNullException("obj");

                var reference = obj as ViewModelReference;
                if (reference != null)
                {
                    return reference.Guid.GetHashCode();
                }
                var referenceList = obj as IList<ViewModelReference>;
                if (referenceList != null)
                {
                    return ArrayExtensions.ComputeHash(referenceList, this);
                }

                return obj.GetHashCode();
            }
        }
    }
}