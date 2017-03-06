using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface INodePresenter : IDisposable
    {
        string Name { get; }

        INodePresenter Parent { get; }

        IReadOnlyList<INodePresenter> Children { get; }

        List<INodeCommand> Commands { get; }

        Type Type { get; }

        bool IsPrimitive { get; }

        Index Index { get; }

        ITypeDescriptor Descriptor { get; }

        int? Order { get; }

        object Value { get; set; }

        event EventHandler<ValueChangingEventArgs> ValueChanging;

        event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
}
