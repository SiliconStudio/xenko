using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract(Inherited = true)]
    public abstract class Block : IIdentifiable, IAssetPartDesign<Block>
    {
        protected Block()
        {
            Id = Guid.NewGuid();
            Slots.CollectionChanged += Slots_CollectionChanged;
        }

        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the base entity in case of prefabs. If null, the entity is not a prefab.
        /// </summary>
        [DataMember(-90), Display(Browsable = false)]
        [DefaultValue(null)]
        public Guid? BaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the part group in case of prefabs. If null, the entity doesn't belong to a part.
        /// </summary>
        [DataMember(-80), Display(Browsable = false)]
        [DefaultValue(null)]
        public Guid? BasePartInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the position of the block.
        /// </summary>
        [DataMember(-50), Display(Browsable = false)]
        public Int2 Position { get; set; }

        /// <summary>
        /// Gets the title of that node, as it will be displayed in the editor.
        /// </summary>
        [DataMemberIgnore]
        public virtual string Title => null;

        /// <summary>
        /// Gets the list of slots this block has.
        /// </summary>
        [DataMember(10000), Display(Browsable = false)]
        public ObservableCollection<Slot> Slots { get; } = new ObservableCollection<Slot>();

        public abstract void RegenerateSlots();

        /// <inheritdoc/>
        Block IAssetPartDesign<Block>.Part => this;

        protected virtual void OnSlotAdd(Slot slot)
        {
            slot.Owner = this;
        }

        protected virtual void OnSlotRemove(Slot slot)
        {
            slot.Owner = null;
        }

        private void Slots_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Slot slot in e.NewItems)
                    {
                        OnSlotAdd(slot);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Slot slot in e.OldItems)
                    {
                        OnSlotRemove(slot);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Note: we should properly disconnect removed slots, but the event doesn't give us this info...
                    foreach (var slot in (IEnumerable<Slot>)sender)
                    {
                        OnSlotAdd(slot);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public abstract class ExecutionBlock : Block
    {
        [DataMemberIgnore]
        public Slot ExecutionInput { get; private set; }

        [DataMemberIgnore]
        public Slot ExecutionOutput { get; private set; }

        public abstract void GenerateCode(VisualScriptCompilerContext context);

        protected override void OnSlotAdd(Slot slot)
        {
            base.OnSlotAdd(slot);

            if (slot.Kind == SlotKind.Execution)
            {
                // Detect default input slot with its null name (should we have a default flag instead?)
                if (slot.Direction == SlotDirection.Input && slot.Name == null)
                    ExecutionInput = slot;

                // Detect default output slot with its null name or auto flow flag (should we have a default flag instead?)
                if (slot.Direction == SlotDirection.Output && (slot.Name == null || (slot.Flags & SlotFlags.AutoflowExecution) != 0))
                    ExecutionOutput = slot;
            }
        }

        protected override void OnSlotRemove(Slot slot)
        {
            if (slot == ExecutionInput)
                ExecutionInput = null;
            if (slot == ExecutionOutput)
                ExecutionOutput = null;

            base.OnSlotRemove(slot);
        }
    }

    public abstract class ExpressionBlock : Block
    {
        public abstract ExpressionSyntax GenerateExpression();
    }

    public class FunctionStartBlock : ExecutionBlock
    {
        public const string StartSlotName = "Start";

        [DataMemberIgnore]
        public Slot StartSlot { get; private set; }

        public override string Title => $"{FunctionName} Start";

        public string FunctionName { get; set; }

        public override void RegenerateSlots()
        {
            Slots.Clear();
            Slots.Add(StartSlot = new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Output, Name = StartSlotName, Flags = SlotFlags.AutoflowExecution });
        }

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            // Nothing to do (since we have autoflow on Start slot)
        }
    }

    public enum SlotKind
    {
        Value = 0,
        Execution = 1,
    }

    public enum SlotDirection
    {
        Input = 0,
        Output = 1,
    }

    [Flags]
    public enum SlotFlags
    {
        None = 0,
        AutoflowExecution = 1,
    }
}