// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Dirtiables;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.Transactions;

namespace SiliconStudio.Presentation.ViewModel
{
    public abstract class EditableViewModel : DispatcherViewModel
    {
        private readonly Dictionary<string, object> preEditValues = new Dictionary<string, object>();
        private readonly HashSet<string> uncancellableChanges = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="IUndoRedoService"/> to use for this view model.</param>
        protected EditableViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            if (serviceProvider.TryGet<IUndoRedoService>() == null)
                throw new ArgumentException("The given IViewModelServiceProvider instance does not contain an service implementing IUndoRedoService.");
        }
        
        public abstract IEnumerable<IDirtiable> Dirtiables { get; }

        /// <summary>
        /// Gets the undo/redo service used by this view model.
        /// </summary>
        public IUndoRedoService ActionService => ServiceProvider.Get<IUndoRedoService>();

        protected void RegisterMemberCollectionForActionStack(string name, INotifyCollectionChanged collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            collection.CollectionChanged += (sender, e) => CollectionChanged(sender, e, name);
        }

        protected bool SetValueUncancellable<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(ref field, value, null, new[] { propertyName });
        }

        protected bool SetValueUncancellable<T>(ref T field, T value, Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(ref field, value, updateAction, new[] { propertyName });
        }

        protected bool SetValueUncancellable<T>(ref T field, T value, params string[] propertyNames)
        {
            return SetValueUncancellable(ref field, value, null, propertyNames);
        }

        protected bool SetValueUncancellable<T>(ref T field, T value, Action updateAction, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                uncancellableChanges.Add(propertyName);
                string[] dependentProperties;
                if (DependentProperties.TryGetValue(propertyName, out dependentProperties))
                {
                    dependentProperties.ForEach(x => uncancellableChanges.Add(x));
                }
            }
            try
            {
                var result = SetValue(ref field, value, updateAction, propertyNames);
                return result;
            }
            finally
            {
                foreach (var propertyName in propertyNames)
                {
                    uncancellableChanges.Remove(propertyName);
                    string[] dependentProperties;
                    if (DependentProperties.TryGetValue(propertyName, out dependentProperties))
                    {
                        dependentProperties.ForEach(x => uncancellableChanges.Remove(x));
                    }
                }
            }
        }

        protected bool SetValueUncancellable(Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(null, updateAction, new[] { propertyName });
        }

        protected bool SetValueUncancellable(Action updateAction, params string[] propertyNames)
        {
            return SetValueUncancellable(null, updateAction, propertyNames);
        }

        protected bool SetValueUncancellable(Func<bool> hasChangedFunction, Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(hasChangedFunction, updateAction, new[] { propertyName });
        }

        protected bool SetValueUncancellable(bool hasChanged, Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(() => hasChanged, updateAction, new[] { propertyName });
        }

        protected bool SetValueUncancellable(bool hasChanged, Action updateAction, params string[] propertyNames)
        {
            return SetValueUncancellable(() => hasChanged, updateAction, propertyNames);
        }

        protected virtual bool SetValueUncancellable(Func<bool> hasChangedFunction, Action updateAction, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                uncancellableChanges.Add(propertyName);
                string[] dependentProperties;
                if (DependentProperties.TryGetValue(propertyName, out dependentProperties))
                {
                    dependentProperties.ForEach(x => uncancellableChanges.Add(x));
                }
            }
            try
            {
                var result = SetValue(hasChangedFunction, updateAction, propertyNames);
                return result;
            }
            finally
            {
                foreach (var propertyName in propertyNames)
                {
                    uncancellableChanges.Remove(propertyName);
                    string[] dependentProperties;
                    if (DependentProperties.TryGetValue(propertyName, out dependentProperties))
                    {
                        dependentProperties.ForEach(x => uncancellableChanges.Remove(x));
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override bool SetValue<T>(ref T field, T value, Action updateAction, params string[] propertyNames)
        {
            if (propertyNames.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(propertyNames), @"This method must be invoked with at least one property name.");

            if (EqualityComparer<T>.Default.Equals(field, value) == false)
            {
                using (var transaction = ActionService.CreateTransaction())
                {
                    var concatPropertyName = string.Join(", ", propertyNames.Where(x => !uncancellableChanges.Contains(x)).Select(s => $"'{s}'"));
                    ActionService.SetName(transaction, $"Update property {concatPropertyName}");
                    return base.SetValue(ref field, value, updateAction, propertyNames);
                }
            }
            return false;
        }

        /// <inheritdoc/>
        protected override bool SetValue(Func<bool> hasChangedFunction, Action updateAction, params string[] propertyNames)
        {
            if (propertyNames.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(propertyNames), @"This method must be invoked with at least one property name.");

            if (hasChangedFunction == null || hasChangedFunction())
            {
                using (var transaction = ActionService.CreateTransaction())
                {
                    var concatPropertyName = string.Join(", ", propertyNames.Where(x => !uncancellableChanges.Contains(x)).Select(s => $"'{s}'"));
                    ActionService.SetName(transaction, $"Update property {concatPropertyName}");
                    return base.SetValue(hasChangedFunction, updateAction, propertyNames);
                }
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanging(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames.Where(x => x != "IsDirty" && !uncancellableChanges.Contains(x)))
            {
                var propertyInfo = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if (propertyInfo?.GetSetMethod() != null && propertyInfo.GetSetMethod().IsPublic)
                {
                    preEditValues.Add(propertyName, propertyInfo.GetValue(this));
                }
            }

            base.OnPropertyChanging(propertyNames);
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(params string[] propertyNames)
        {
            base.OnPropertyChanged(propertyNames);

            foreach (string propertyName in propertyNames.Where(x => x != "IsDirty" && !uncancellableChanges.Contains(x)))
            {
                string displayName = $"Updated '{propertyName}'";
                object preEditValue;
                if (preEditValues.TryGetValue(propertyName, out preEditValue) && !uncancellableChanges.Contains(propertyName))
                {
                    var propertyInfo = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                    var postEditValue = propertyInfo.GetValue(this);
                    if (!ActionService.UndoRedoInProgress && !Equals(preEditValue, postEditValue))
                    {
                        var operation = CreatePropertyChangeOperation(displayName, propertyName, preEditValue);
                        ActionService.PushOperation(operation);
                    }
                }
                preEditValues.Remove(propertyName);
            }
        }

        protected virtual Operation CreatePropertyChangeOperation(string displayName, string propertyName, object preEditValue)
        {
            var operation = new PropertyChangeOperation(propertyName, this, preEditValue, Dirtiables);
            ActionService.SetName(operation, displayName);
            return operation;
        }

        protected virtual Operation CreateCollectionChangeActionItem(string displayName, IList list, NotifyCollectionChangedEventArgs args)
        {
            var operation = new CollectionChangeOperation(list, args, Dirtiables);
            ActionService.SetName(operation, displayName);
            return operation;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, string collectionName)
        {
            string displayName = $"Updated collection '{collectionName}' ({e.Action})";
            var list = sender as IList;
            if (list == null)
            {
                var toIListMethod = sender.GetType().GetMethod("ToIList");
                if (toIListMethod != null)
                    list = (IList)toIListMethod.Invoke(sender, new object[0]);
            }
            using (ActionService.CreateTransaction())
            {
                var operation = CreateCollectionChangeActionItem(displayName, list, e);
                ActionService.PushOperation(operation);
            }
        }
    }
}
