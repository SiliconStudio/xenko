// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;

using SiliconStudio.Presentation.Commands;
using SiliconStudio.Core;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Presentation.Quantum.Legacy
{
    public abstract class ObservableViewModelNode : ViewModelNode, INotifyPropertyChanged
    {
        protected ObservableViewModelNode(ViewModelContext context, IViewModelNode modelNode)
        {
            ModelNode = modelNode;
            Guid = ModelNode.Guid;
            Name = ModelNode.Name;
            Context = context;
            PropertyContainer = new PropertyContainer(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IViewModelNode ModelNode { get; private set; }

        public ViewModelContext Context { get; set; }

        public PropertyContainer PropertyContainer;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract void RefreshValue();

        public static ObservableViewModelNode CreateObservableViewModel(ViewModelContext contextUI, IViewModelNode viewModelNode)
        {
            ObservableViewModelNode propertyView;
            if (viewModelNode.Content.Type == typeof(ExecuteCommand))
            {
                // TODO: not supported anymore!
                //propertyView = new ObservableViewModelNode<INodeCommand>(contextUI, viewModelNode);
                propertyView = null;
            }
            else
                propertyView = (ObservableViewModelNode)Activator.CreateInstance(typeof(ObservableViewModelNode<>).MakeGenericType(viewModelNode.Content.Type), contextUI, viewModelNode);

            foreach (var child in viewModelNode.Children)
            {
                propertyView.Children.Add(CreateObservableViewModel(contextUI, child));
            }

            return propertyView;
        }

        static void RefreshRecursive(ObservableViewModelNode observableViewModel)
        {
            observableViewModel.RefreshValue();

            foreach (ObservableViewModelNode child in observableViewModel.Children)
            {
                RefreshRecursive(child);
            }
        }

        public static void ForceRefresh(ViewModelContext contextUI, ViewModelContext context, ViewModelState state)
        {
            lock (context)
            {
                // Remove unused UI viewmodels
                foreach (var viewModelUI in contextUI.ViewModelByGuid.ToArray())
                {
                    if (!context.ViewModelByGuid.ContainsKey(viewModelUI.Value.Guid))
                    {
                        contextUI.ViewModelByGuid.Remove(viewModelUI.Key);
                    }
                }

                foreach (var viewModel in context.ViewModelByGuid)
                {
                    IViewModelNode oldViewModel;
                    bool newVersion = !state.ViewModelByGuid.TryGetValue(viewModel.Key, out oldViewModel) || oldViewModel != viewModel.Value;
                    if (newVersion)
                    {
                        var viewModelUI = CreateObservableViewModel(contextUI, viewModel.Value);
                        contextUI.ViewModelByGuid[viewModelUI.Guid] = viewModelUI;
                    }

                    // Update root (if found)
                    if (context.Root == viewModel.Value)
                        contextUI.Root = contextUI.ViewModelByGuid[viewModel.Key];
                }

                state.ViewModelByGuid = new Dictionary<Guid, IViewModelNode>(context.ViewModelByGuid);

                // Update observable viewmodels.
                foreach (var viewModel in context.ViewModelByGuid)
                {
                    var viewModelUI = (ObservableViewModelNode)contextUI.ViewModelByGuid[viewModel.Value.Guid];

                    RefreshRecursive(viewModelUI);
                }

                context.CurrentGuids = null;
            }
        }

        public static void Refresh(ViewModelContext contextUI, ViewModelContext context, ViewModelState state)
        {
            lock (context)
            {
                // No available update
                if (context.CurrentGuids == null)
                    return;

                ForceRefresh(contextUI, context, state);
            }
        }

        protected IViewModelNode GetChildInternal(string name)
        {
            return Children.FirstOrDefault(x => x.Name == name);
        }

        public IViewModelNode GetChild(string name)
        {
            return GetChildInternal(name);
        }

        /// <summary>
        /// Add a dependency between this node and the given one. When a change on <cref see="dependencyNode" /> is notified, it will be forwarded to this node which will notify a change on itself
        /// </summary>
        /// <param name="dependencyNode">The dependency node.</param>
        public void AddDependency(ObservableViewModelNode dependencyNode)
        {
            dependencyNode.PropertyChanged += PropagatePropertyChanged;
        }

        /// <summary>
        /// Remove a dependency added by <cref see="AddDependency" />
        /// </summary>
        /// <param name="dependencyNode"></param>
        public void RemoveDependency(ObservableViewModelNode dependencyNode)
        {
            dependencyNode.PropertyChanged -= PropagatePropertyChanged;
        }

        /// <summary>
        /// Event handler used by <cref see="AddDependency" /> to propagate a <cref see="PropertyChanged" /> event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void PropagatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshValue();
            RaisePropertyChanged(e.PropertyName);
        }
    }

    public class ObservableViewModelNode<T> : ObservableViewModelNode, IDynamicMetaObjectProvider
    {
        private readonly ObservableViewModelContent<T> content;
        private bool ignoreCollectionChanged;

        public ObservableViewModelNode(ViewModelContext context, IViewModelNode modelNode)
            : base(context, modelNode)
        {
            Content = content = new ObservableViewModelContent<T>(modelNode.Content.IsReadOnly);

            // Force refresh (trigger copy if network value)
            // TODO: FIXME
            // ModelNode.Content.IsValueUpdated();

            if (ModelNode.Content.Type == typeof(ViewModelReference))
            {
                content.TValue = default(T);
            }
            else if (ModelNode.Content.Type == typeof(IList<ViewModelReference>))
            {
                var collection = new ObservableCollection<ViewModelReference>();
                collection.CollectionChanged += OnCollectionChanged;
                content.TValue = (T)(object)collection;
            }
            else if (ModelNode.Content.Type.IsGenericType && ModelNode.Content.Type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                //Type itemType = typeof(ObservableViewModelNode<>).MakeGenericType(ModelNode.Type.GetGenericArguments()[0]);
                Type itemType = ModelNode.Content.Type.GetGenericArguments()[0];
                Type collectionType = typeof(ObservableCollection<>).MakeGenericType(itemType);
                var collection = Activator.CreateInstance(collectionType);
                content.TValue = (T)collection;
            }
            else if (ModelNode.Content.Type == typeof(ExecuteCommand))
            {
                content.TValue = (T)(object)new AnonymousCommand(null, parameter =>
                {
                    var executeMethod = (ExecuteCommand)ModelNode.Content.Value;
                    executeMethod(ModelNode, parameter);

                    var parent = (ObservableViewModelNode)Parent;
                    parent.RefreshValue();
                });
            }
            else
            {
                var combinedContent = ModelNode.Content as CombinedViewModelContent;
                if (combinedContent != null && (combinedContent.Flags & ViewModelContentFlags.CombineError) == ViewModelContentFlags.CombineError)
                {
                    content.TValue = default(T);
                }
                else
                {
                    content.TValue = (T)ModelNode.Content.Value;
                }
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ignoreCollectionChanged)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public ViewModelContentState LoadState
        {
            get
            {
                return Content.LoadState;
            }
            set
            {
                Content.LoadState = value;
                if (value != ModelNode.Content.LoadState)
                    ModelNode.Content.LoadState = value;

                RaisePropertyChanged("LoadState");
            }
        }

        private ICommand loadContentCommand;
        public ICommand LoadContentCommand
        {
            get
            {
                if (loadContentCommand == null)
                    loadContentCommand = new AnonymousCommand(null, LoadContent);
                return loadContentCommand;
            }
        }

        private ICommand cancelContentLoadingCommand;
        public ICommand CancelContentLoadingCommand
        {
            get
            {
                if (cancelContentLoadingCommand == null)
                    cancelContentLoadingCommand = new AnonymousCommand(null, CancelContentLoading);
                return cancelContentLoadingCommand;
            }
        }

        private void LoadContent()
        {
            // TODO: Cast for now, but should be part of itnerface
            if (ModelNode.Content is IAsyncViewModelContent)
            {
                ((IAsyncViewModelContent)ModelNode.Content).RequestLoadContent();
            }
        }

        private void CancelContentLoading()
        {
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T TValue
        {
            get
            {
                return content.TValue;
            }
            set
            {
                if (object.Equals(((ObservableViewModelContent<T>)Content).TValue, value))
                    return;

                content.TValue = value;
                ModelNode.Content.Value = content.TValue;

                RaisePropertyChanged("TValue");
                RaisePropertyChanged("Value");

                // TODO: Update children
            }
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicPropertyViewMetaObject(parameter, this);
        }

        private class DynamicPropertyViewMetaObject : DynamicMetaObject
        {
            public DynamicPropertyViewMetaObject(Expression parameter, ObservableViewModelNode<T> observableViewModel)
                : base(parameter, BindingRestrictions.Empty, observableViewModel)
            {
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                // Parse Attached_[Class]_[AttachedProperty]
                if (binder.Name.StartsWith("Attached_"))
                {
                    var self = Expression.Convert(this.Expression, this.LimitType);

                    var parameters = binder.Name.Split('_');
                    var className = parameters[1];
                    var attachedPropertyName = parameters[2];

                    var classType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).First(x => x.Name == className);

                    
                    var expression = Expression.Block(Expression.Call(classType.GetMethod("Set" + attachedPropertyName), self, Expression.Convert(value.Expression, value.LimitType)), value.Expression);
                    return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
                }

                return base.BindSetMember(binder, value);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var self = Expression.Convert(this.Expression, this.LimitType);

                if (binder.Name == "Value")
                {
                    return new DynamicMetaObject(Expression.Property(self, "Value"), BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
                }
                if (binder.Name == "LoadState" || binder.Name == "TValue")
                {
                    return base.BindGetMember(binder);
                }

                Expression expression;

                // Parse Attached_[Class]_[AttachedProperty]
                if (binder.Name.StartsWith("Attached_"))
                {
                    var parameters = binder.Name.Split('_');
                    var className = parameters[1];
                    var attachedPropertyName = parameters[2];

                    var classType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(x => x.Name == className);

                    if (classType == null)
                    {
                        expression = Expression.Constant(binder.ReturnType.IsValueType ? Activator.CreateInstance(binder.ReturnType) : null);
                    }
                    else
                    {
                        expression = Expression.Convert(Expression.Call(classType.GetMethod("Get" + attachedPropertyName), self), binder.ReturnType);
                    }
                }
                else
                {
                    var propertyName = binder.Name;

                    if (binder.Name.StartsWith("HasChild"))
                    {
                        propertyName = binder.Name.Substring("HasChild".Length);
                    }

                    var args = new Expression[1];
                    args[0] = Expression.Constant(propertyName);

                    expression = Expression.Call(self, typeof(ObservableViewModelNode<T>).GetMethod("GetChildInternal", BindingFlags.NonPublic | BindingFlags.Instance), args);

                    if (binder.Name.StartsWith("HasChild"))
                    {
                        expression = Expression.Convert(Expression.NotEqual(expression, Expression.Constant(null)), binder.ReturnType);
                    }
                }

                var getChild = new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
                return getChild;
            }
        }

        /// <inheritdoc/>
        public override void RefreshValue()
        {
            bool updated = ModelNode.Content.UpdatedValue != ContentBase.ValueNotUpdated;

            if (ModelNode.Content.Type == typeof(ViewModelReference))
            {
                if (ReferenceEquals(content.TValue, null) || ((ViewModelReference)ModelNode.Content.Value).Guid != ((ViewModelReference)(object)content.TValue).Guid)
                {
                    content.TValue = (T)(object)new ViewModelReference(((ViewModelReference)ModelNode.Content.Value).Guid, Context);
                }
            }
            else if (ModelNode.Content.Type == typeof(IList<ViewModelReference>))
            {
                // Handle List update through ObservableCollection (still not so evolved yet)
                var list2 = (IList<ViewModelReference>)ModelNode.Content.Value;
                if (list2 != null)
                {
                    ignoreCollectionChanged = true;

                    // Apply diff
                    var comparer = new ViewModelReferenceGuidComparer();
                    var list1 = (IList<ViewModelReference>)content.TValue;
                    var backtrack = LCS.GetLCS(list1, list2, comparer);
                    var diff = LCS.GenerateDiff(backtrack, list1, list2, comparer);
                    LCS.ApplyDiff(list1, list2, diff, x => new ViewModelReference(x.Guid, Context));

                    ignoreCollectionChanged = false;

                    //foreach (var item in list1)
                    //{
                    //    item.UpdateViewModel();
                    //}
                }
            }
            else if (ModelNode.Content.Type.IsGenericType && ModelNode.Content.Type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var genericType = ModelNode.Content.Type.GetGenericArguments()[0];

                var list2 = ModelNode.Content.Value;
                if (list2 != null)
                {
                    ignoreCollectionChanged = true;

                    // Apply diff
                    //Type itemType = typeof(ObservableViewModelNode<>).MakeGenericType(ModelNode.Content.Type.GetGenericArguments()[0]);
                    //var list1 = Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType), new[] { content.TValue });
                    var list1 = content.TValue;
                    var backtrack = typeof(LCS).GetMethod("GetLCS").MakeGenericMethod(genericType).Invoke(null, new[] { list1, list2, null });
                    var diff = typeof(LCS).GetMethod("GenerateDiff").MakeGenericMethod(genericType).Invoke(null, new[] { backtrack, list1, list2, null });
                    typeof(LCS).GetMethod("ApplyDiff").MakeGenericMethod(genericType).Invoke(null, new[] { content.TValue, list2, diff, null });

                    ignoreCollectionChanged = false;
                }
            }
            else if (ModelNode.Content.Type == typeof(ExecuteCommand))
            {

            }
            else
            {
                var combinedContent = ModelNode.Content as CombinedViewModelContent;
                if (combinedContent != null && (combinedContent.Flags & ViewModelContentFlags.CombineError) == ViewModelContentFlags.CombineError)
                {
                    content.TValue = default(T);
                }
                else
                {
                    content.TValue = (T)ModelNode.Content.Value;
                }
            }

            if (updated)
            {
                RaisePropertyChanged("TValue");
                RaisePropertyChanged("Value");
            }

            content.Flags = ModelNode.Content.Flags;

            if (content.LoadState != ModelNode.Content.LoadState)
            {
                // If loading request but not arrived yet, ignore changes.
                if (!(content.LoadState == ViewModelContentState.Loading && ModelNode.Content.LoadState == ViewModelContentState.NotLoaded))
                {
                    LoadState = ModelNode.Content.LoadState;
                }
            }
        }
    }
}