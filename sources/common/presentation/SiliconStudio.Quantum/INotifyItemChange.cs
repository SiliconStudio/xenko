using System;

namespace SiliconStudio.Quantum
{
    public interface INotifyItemChange
    {
        event EventHandler<ItemChangeEventArgs> ItemChanging;

        event EventHandler<ItemChangeEventArgs> ItemChanged;
    }
}