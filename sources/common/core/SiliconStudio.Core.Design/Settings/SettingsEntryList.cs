// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An internal object that represent a list of typed values for a settings key into a <see cref="SettingsProfile"/>.
    /// </summary>
    /// <typeparam name="T">The type of values contained in the list.</typeparam>
    internal sealed class SettingsEntryList<T> : SettingsEntry
    {
        private readonly TrackingCollection<T> items = new TrackingCollection<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEntryList{T}"/> class.
        /// </summary>
        /// <param name="profile">The profile this <see cref="SettingsEntryList{T}"/>belongs to.</param>
        /// <param name="name">The name associated to this <see cref="SettingsEntryList{T}"/>.</param>
        /// <param name="listItems">The items to associate to this <see cref="SettingsEntryList{T}"/>.</param>
        public SettingsEntryList(SettingsProfile profile, UFile name, IEnumerable listItems)
            : base(profile, name)
        {
            if (listItems == null) throw new ArgumentNullException("listItems");
            foreach (T item in listItems)
                items.Add(item);
            Value = items;
            items.CollectionChanged += CollectionChanged;
            ShouldNotify = true;
        }

        /// <inheritdoc/>
        internal override object GetSerializableValue()
        {
            return new List<object>(items.Cast<object>().Select(x => x != null ? string.Format(CultureInfo.InvariantCulture, "{0}", x) : null));
        }

        private void CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            if (!Profile.IsDiscarding)
            {

                CollectionChangedActionItem action;
                
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                        action = new CollectionChangedActionItem(items, new NotifyCollectionChangedEventArgs(e.Action, e.Item, e.Index));
                        break;
                    default:
                        throw new NotSupportedException();
                }
                Profile.ActionStack.Add(action);
                Profile.NotifyEntryChanged(Name);
            }
        }
    }
}