using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.ViewModels;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenter : IDisposable
    {
        INodePresenter this[string childName] { get; }

        string DisplayName { get; set; }

        string Name { get; }

        INodePresenter Root { get; }

        INodePresenter Parent { get; }

        IReadOnlyList<INodePresenter> Children { get; }

        List<INodePresenterCommand> Commands { get; }

        PropertyContainerClass AttachedProperties { get; }

        Type Type { get; }

        bool IsPrimitive { get; }

        bool IsEnumerable { get; }

        bool IsReadOnly { get; set; }

        bool IsVisible { get; set; }

        Index Index { get; }

        ITypeDescriptor Descriptor { get; }

        int? CustomOrder { set; get; }

        int? Order { get; }

        object Value { get; }

        string CombineKey { get; }

        IPropertyProviderViewModel PropertyProvider { get; }

        INodePresenterFactory Factory { get; }

        event EventHandler<ValueChangingEventArgs> ValueChanging;

        event EventHandler<ValueChangedEventArgs> ValueChanged;

        void UpdateValue(object newValue);

        void AddItem(object value);

        void AddItem(object value, Index index);

        void RemoveItem(object value, Index index);

        // TODO: this should probably be removed, UpdateValue should be called on the corresponding child node presenter itself

        NodeAccessor GetNodeAccessor();

        /// <summary>
        /// Adds a dependency to the given node.
        /// </summary>
        /// <param name="node">The node that should be a dependency of this node.</param>
        /// <param name="refreshOnNestedNodeChanges">If true, this node will also be refreshed when one of the child node of the dependency node changes.</param>
        /// <remarks>A node that is a dependency to this node will trigger a refresh of this node each time its value is modified (or the value of one of its parent).</remarks>
        void AddDependency(INodePresenter node, bool refreshOnNestedNodeChanges);

        void ChangeParent([NotNull] INodePresenter newParent);

        void Rename(string newName);
    }
}
