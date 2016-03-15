// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// This class is an implementation of the <see cref="DispatcherViewModel"/> class that supports undo/redo of property and collection changes.
    /// It requires an <see cref="ITransactionalActionStack"/> and can be linked to one or several <see cref="IDirtiable"/> objects.
    /// The dirtiable objects will have their dirty flag updated accordingly to the state of the action stack.
    /// </summary>
    /// <remarks>
    /// When one of the <c>SetValue</c> methods is invoked, it will automatically create an <see cref="IActionItem"/> and add it to the
    /// registered <see cref="ITransactionalActionStack"/>. To modify a property without creating action items, use one of the <c>SetValueUncancellable</c>
    /// methods, such as <see cref="SetValueUncancellable{T}(ref T, T, string)"/>.
    /// </remarks>
    /// <remarks>This class is abstract because it does not provide a default implementation of the <see cref="Dirtiables"/> property.</remarks>
    public abstract class EditableViewModel : DispatcherViewModel
    {
        private readonly Dictionary<string, object> preEditValues = new Dictionary<string, object>();
        private readonly HashSet<string> uncancellableChanges = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ITransactionalActionStack"/> to use for this view model.</param>
        protected EditableViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            if (serviceProvider.TryGet<ITransactionalActionStack>() == null)
                throw new ArgumentException("The given IViewModelServiceProvider instance does not contain an ITransactionalActionStack service.");
        }
        
        /// <summary>
        /// Gets the list of <see cref="IDirtiable"/> objects linked to this view model. 
        /// </summary>
        /// <remarks>Dirtiable objects will have their dirty flag updated when a change occurs or when the action stack is notified that modifications have been saved.</remarks>
        public abstract IEnumerable<IDirtiable> Dirtiables { get; }

        /// <summary>
        /// Gets the transactional action stack used by this view model.
        /// </summary>
        public ITransactionalActionStack ActionStack => ServiceProvider.Get<ITransactionalActionStack>();

        /// <summary>
        /// Registers the given collection to create <see cref="CollectionChangedActionItem"/> in the action stack when it is modified.
        /// </summary>
        /// <param name="name">The name of the collection (used only for formatting the display name of the action item).</param>
        /// <param name="collection">The collection to register.</param>
        protected void RegisterMemberCollectionForActionStack(string name, INotifyCollectionChanged collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            collection.CollectionChanged += (sender, e) => CollectionChanged(sender, e, name);
        }

        /// <summary>
        /// Sets the value of a field to the given value. Both values are compared with the default <see cref="EqualityComparer{T}"/>, and if they are equals,
        /// this method does nothing. If they are different, the <see cref="ViewModelBase.PropertyChanging"/> event will be raised first, then the field value will be modified,
        /// and finally the <see cref="ViewModelBase.PropertyChanged"/> event will be raised.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">A reference to the field to set.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property that must be notified as changing/changed. Can use <see cref="CallerMemberNameAttribute"/>.</param>
        /// <returns><c>True</c> if the field was modified and events were raised, <c>False</c> if the new value was equal to the old one and nothing was done.</returns>
        protected bool SetValueUncancellable<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(ref field, value, null, new[] { propertyName });
        }

        /// <summary>
        /// Sets the value of a field to the given value. Both values are compared with the default <see cref="EqualityComparer{T}"/>, and if they are equals,
        /// this method does nothing. If they are different, the <see cref="ViewModelBase.PropertyChanging"/> event will be raised first, then the field value will be modified.
        /// The given update action will be executed and finally the <see cref="ViewModelBase.PropertyChanged"/> event will be raised.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">A reference to the field to set.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="updateAction">The update action to execute after setting the value. Can be <c>null</c>.</param>
        /// <param name="propertyName">The name of the property that must be notified as changing/changed. Can use <see cref="CallerMemberNameAttribute"/>.</param>
        /// <returns><c>True</c> if the field was modified and events were raised, <c>False</c> if the new value was equal to the old one and nothing was done.</returns>
        protected bool SetValueUncancellable<T>(ref T field, T value, Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(ref field, value, updateAction, new[] { propertyName });
        }

        /// <summary>
        /// Sets the value of a field to the given value. Both values are compared with the default <see cref="EqualityComparer{T}"/>, and if they are equals,
        /// this method does nothing. If they are different, the <see cref="ViewModelBase.PropertyChanging"/> will be raised first, then the field value will be modified,
        /// and finally the <see cref="ViewModelBase.PropertyChanged"/> event will be raised.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">A reference to the field to set.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyNames">The names of the properties that must be notified as changing/changed. At least one property name must be provided.</param>
        /// <returns><c>True</c> if the field was modified and events were raised, <c>False</c> if the new value was equal to the old one and nothing was done.</returns>
        protected bool SetValueUncancellable<T>(ref T field, T value, params string[] propertyNames)
        {
            return SetValueUncancellable(ref field, value, null, propertyNames);
        }

        /// <summary>
        /// Sets the value of a field to the given value. Both values are compared with the default <see cref="EqualityComparer{T}"/>, and if they are equals,
        /// this method does nothing. If they are different, the <see cref="ViewModelBase.PropertyChanging"/> event will be raised first, then the field value will be modified.
        /// The given update action will be executed and finally the <see cref="ViewModelBase.PropertyChanged"/> event will be raised.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">A reference to the field to set.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="updateAction">The update action to execute after setting the value. Can be <c>null</c>.</param>
        /// <param name="propertyNames">The names of the properties that must be notified as changing/changed. At least one property name must be provided.</param>
        /// <returns><c>True</c> if the field was modified and events were raised, <c>False</c> if the new value was equal to the old one and nothing was done.</returns>
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

        /// <summary>
        /// Manages a property modification and its notifications. This method will invoke the provided update action. The <see cref="ViewModelBase.PropertyChanging"/>
        /// event will be raised prior to the update action, and the <see cref="ViewModelBase.PropertyChanged"/> event will be raised after.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <param name="updateAction">The update action that will actually manage the update of the property.</param>
        /// <param name="propertyName">The name of the property that must be notified as changing/changed. Can use <see cref="CallerMemberNameAttribute"/>.</param>
        /// <returns>This method always returns<c>True</c> since it always performs the update.</returns>
        protected bool SetValueUncancellable(Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(null, updateAction, new[] { propertyName });
        }

        /// <summary>
        /// Manages a property modification and its notifications. This method will invoke the provided update action. The <see cref="ViewModelBase.PropertyChanging"/>
        /// event will be raised prior to the update action, and the <see cref="ViewModelBase.PropertyChanged"/> event will be raised after.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <param name="updateAction">The update action that will actually manage the update of the property.</param>
        /// <param name="propertyNames">The names of the properties that must be notified as changing/changed. At least one property name must be provided.</param>
        /// <returns>This method always returns<c>True</c> since it always performs the update.</returns>
        protected bool SetValueUncancellable(Action updateAction, params string[] propertyNames)
        {
            return SetValueUncancellable(null, updateAction, propertyNames);
        }

        /// <summary>
        /// Manages a property modification and its notifications. A function is provided to check whether the new value is different from the current one.
        /// This function will be invoked by this method, and if it returns <c>True</c>, it will invoke the provided update action. The <see cref="ViewModelBase.PropertyChanging"/>
        /// event will be raised prior to the update action, and the <see cref="ViewModelBase.PropertyChanged"/> event will be raised after.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <param name="hasChangedFunction">A function that check if the new value is different and therefore if the update must be actually done.</param>
        /// <param name="updateAction">The update action that will actually manage the update of the property.</param>
        /// <param name="propertyName">The name of the property that must be notified as changing/changed. Can use <see cref="CallerMemberNameAttribute"/>.</param>
        /// <returns><c>True</c> if the update was done and events were raised, <c>False</c> if <see cref="hasChangedFunction"/> is not <c>null</c> and returned false.</returns>
        protected bool SetValueUncancellable(Func<bool> hasChangedFunction, Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(hasChangedFunction, updateAction, new[] { propertyName });
        }

        /// <summary>
        /// Manages a property modification and its notifications. The first parameter <see cref="hasChanged"/> should indicate whether the property
        /// should actuallybe updated. If this parameter is <c>True</c>, it will invoke the provided update action. The <see cref="ViewModelBase.PropertyChanging"/>
        /// event will be raised prior to the update action, and the <see cref="ViewModelBase.PropertyChanged"/> event will be raised after.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <param name="hasChanged">A boolean that indicates whether the update must be actually done. If <c>null</c>, the update is always done.</param>
        /// <param name="updateAction">The update action that will actually manage the update of the property.</param>
        /// <param name="propertyName">The name of the property that must be notified as changing/changed. Can use <see cref="CallerMemberNameAttribute"/>.</param>
        /// <returns>The value provided in the <see cref="hasChanged"/> argument.</returns>
        protected bool SetValueUncancellable(bool hasChanged, Action updateAction, [CallerMemberName]string propertyName = null)
        {
            return SetValueUncancellable(() => hasChanged, updateAction, new[] { propertyName });
        }

        /// <summary>
        /// Manages a property modification and its notifications. The first parameter <see cref="hasChanged"/> should indicate whether the property
        /// should actuallybe updated. If this parameter is <c>True</c>, it will invoke the provided update action. The <see cref="ViewModelBase.PropertyChanging"/>
        /// event will be raised prior to the update action, and the <see cref="ViewModelBase.PropertyChanged"/> event will be raised after.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <param name="hasChanged">A boolean that indicates whether the update must be actually done. If <c>null</c>, the update is always done.</param>
        /// <param name="updateAction">The update action that will actually manage the update of the property.</param>
        /// <param name="propertyNames">The names of the properties that must be notified as changing/changed. At least one property name must be provided.</param>
        /// <returns>The value provided in the <see cref="hasChanged"/> argument.</returns>
        protected bool SetValueUncancellable(bool hasChanged, Action updateAction, params string[] propertyNames)
        {
            return SetValueUncancellable(() => hasChanged, updateAction, propertyNames);
        }

        /// <summary>
        /// Manages a property modification and its notifications. A function is provided to check whether the new value is different from the current one.
        /// This function will be invoked by this method, and if it returns <c>True</c>, it will invoke the provided update action. The <see cref="ViewModelBase.PropertyChanging"/>
        /// event will be raised prior to the update action, and the <see cref="ViewModelBase.PropertyChanged"/> event will be raised after.
        /// </summary>
        /// <remarks>This method does not register <see cref="IActionItem"/> into the associated <see cref="ITransactionalActionStack"/>.</remarks>
        /// <param name="hasChangedFunction">A function that check if the new value is different and therefore if the update must be actually done.</param>
        /// <param name="updateAction">The update action that will actually manage the update of the property.</param>
        /// <param name="propertyNames">The names of the properties that must be notified as changing/changed. At least one property name must be provided.</param>
        /// <returns><c>True</c> if the update was done and events were raised, <c>False</c> if <see cref="hasChangedFunction"/> is not <c>null</c> and returned false.</returns>
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
                string concatPropertyName = string.Join(", ", propertyNames.Where(x => !uncancellableChanges.Contains(x)).Select(s => $"'{s}'"));
                if (concatPropertyName.Length > 0)
                {
                    ActionStack.BeginTransaction();
                }

                var result = base.SetValue(ref field, value, updateAction, propertyNames);
                
                if (concatPropertyName.Length > 0)
                {
                    ActionStack.EndTransaction("Updated " + concatPropertyName);
                }
                return result;
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
                string concatPropertyName = string.Join(", ", propertyNames.Where(x => !uncancellableChanges.Contains(x)).Select(s => $"'{s}'"));
                if (concatPropertyName.Length > 0)
                {
                    ActionStack.BeginTransaction();
                }

                var result = base.SetValue(hasChangedFunction, updateAction, propertyNames);

                if (concatPropertyName.Length > 0)
                {
                    ActionStack.EndTransaction("Updated " + concatPropertyName);
                }
                return result;
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
                    if (!Equals(preEditValue, postEditValue))
                    {
                        var actionItem = CreatePropertyChangeActionItem(displayName, propertyName, preEditValue);
                        ActionStack.Add(actionItem);
                    }
                }
                preEditValues.Remove(propertyName);
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="DirtiableActionItem"/> corresponding to the action of modifying a property of this view model.
        /// </summary>
        /// <param name="displayName">The display name of the action.</param>
        /// <param name="propertyName">The name of the modified property.</param>
        /// <param name="preEditValue">The value of the property before the modification.</param>
        /// <returns>A new instance of the <see cref="DirtiableActionItem"/> class.</returns>
        protected virtual DirtiableActionItem CreatePropertyChangeActionItem(string displayName, string propertyName, object preEditValue)
        {
            return new PropertyChangedActionItem(displayName, propertyName, this, preEditValue, Dirtiables);
        }

        /// <summary>
        /// Creates an instance of <see cref="DirtiableActionItem"/> corresponding to the action of modifying an observable collection of this view model.
        /// </summary>
        /// <param name="displayName">The display name of the action.</param>
        /// <param name="list">The collection that has been modified.</param>
        /// <param name="args">A <see cref="NotifyCollectionChangedEventArgs"/> object containing the information of the change.</param>
        /// <returns>A new instance of the <see cref="DirtiableActionItem"/> class.</returns>
        protected virtual DirtiableActionItem CreateCollectionChangeActionItem(string displayName, IList list, NotifyCollectionChangedEventArgs args)
        {
            return new CollectionChangedActionItem(displayName, list, args, Dirtiables);
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
            var actionItem = CreateCollectionChangeActionItem(displayName, list, e);
            ActionStack.Add(actionItem);
        }
    }
}
