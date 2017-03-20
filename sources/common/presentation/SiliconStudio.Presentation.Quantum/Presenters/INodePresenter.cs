using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenter : IDisposable
    {
        string Name { get; }

        INodePresenter Root { get; }

        INodePresenter Parent { get; }

        IReadOnlyList<INodePresenter> Children { get; }

        List<INodePresenterCommand> Commands { get; }

        PropertyContainer AttachedProperties { get; }

        Type Type { get; }

        bool IsPrimitive { get; }

        bool IsEnumerable { get; }

        bool IsVisible { get; set; }

        Index Index { get; }

        ITypeDescriptor Descriptor { get; }

        int? Order { get; }

        object Value { get; }

        string CombineKey { get; }

        event EventHandler<ValueChangingEventArgs> ValueChanging;

        event EventHandler<ValueChangedEventArgs> ValueChanged;

        void UpdateValue(object newValue);

        void AddItem(object value);

        void AddItem(object value, Index index);

        void RemoveItem(object value, Index index);

        // TODO: this should probably be removed, UpdateValue should be called on the corresponding child node presenter itself
        void UpdateItem(object newValue, Index index);

        NodeAccessor GetNodeAccessor();

        void ChangeParent([NotNull] INodePresenter newParent);
    }
}
