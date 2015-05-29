// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Presentation.Collections;

namespace SiliconStudio.Presentation.Settings
{
    /// <summary>
    /// An internal object that represent a list of typed values for a settings key into a <see cref="SettingsProfile"/>.
    /// </summary>
    /// <typeparam name="T">The type of values contained in the list.</typeparam>
    internal sealed class SettingsEntryList<T> : SettingsEntry
    {
        private readonly NonGenericObservableListWrapper<T> items = new NonGenericObservableListWrapper<T>(new ObservableList<T>());

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
            listItems.Cast<object>().ForEach(x => items.Add(x));
            Value = items;
            items.CollectionChanged += CollectionChanged;
            ShouldNotify = true;
        }

        /// <inheritdoc/>
        internal override object GetSerializableValue()
        {
            return new List<object>(items.Cast<object>().Select(x => x != null ? string.Format(CultureInfo.InvariantCulture, "{0}", x) : null));
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Profile.IsDiscarding)
            {
                var action = new CollectionChangedActionItem(items, e);
                Profile.ActionStack.Add(action);
                Profile.NotifyEntryChanged(Name);
            }
        }
    }
}