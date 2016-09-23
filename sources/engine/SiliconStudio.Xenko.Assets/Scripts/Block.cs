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
        public TrackingCollection<Slot> Slots { get; } = new TrackingCollection<Slot>();

        public abstract void RegenerateSlots(IList<Slot> newSlots);

        protected Slot FindSlot(SlotDirection direction, SlotKind kind, string name)
        {
            foreach (var slot in Slots)
            {
                if (slot.Direction == direction && slot.Kind == kind && slot.Name == name)
                    return slot;
            }

            return null;
        }

        protected Slot FindSlot(SlotDirection direction, SlotKind kind, SlotFlags flags)
        {
            foreach (var slot in Slots)
            {
                if (slot.Direction == direction && slot.Kind == kind && (slot.Flags & flags) == flags)
                    return slot;
            }

            return null;
        }

        protected Slot FindSlot(SlotDefinition definition)
        {
            foreach (var slot in Slots)
            {
                if (slot.Direction == definition.Direction && slot.Kind == definition.Kind && slot.Name == definition.Name && slot.Flags == definition.Flags)
                    return slot;
            }

            return null;
        }

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

        private void Slots_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnSlotAdd((Slot)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnSlotRemove((Slot)e.Item);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public abstract class ExecutionBlock : Block
    {
        [DataMemberIgnore]
        public virtual Slot InputExecution => FindSlot(InputExecutionSlotDefinition);

        [DataMemberIgnore]
        public virtual Slot OutputExecution => FindSlot(OutputExecutionSlotDefinition);

        public static readonly SlotDefinition InputExecutionSlotDefinition = SlotDefinition.NewExecutionInput(null);
        public static readonly SlotDefinition OutputExecutionSlotDefinition = SlotDefinition.NewExecutionOutput(null, SlotFlags.AutoflowExecution);

        public abstract void GenerateCode(VisualScriptCompilerContext context);
    }

    public interface IExpressionBlock
    {
        ExpressionSyntax GenerateExpression(VisualScriptCompilerContext context, Slot slot);
    }

    public abstract class ExpressionBlock : Block, IExpressionBlock
    {
        ExpressionSyntax IExpressionBlock.GenerateExpression(VisualScriptCompilerContext context, Slot slot)
        {
            return GenerateExpression();
        }

        public abstract ExpressionSyntax GenerateExpression();
    }

    public class FunctionStartBlock : ExecutionBlock
    {
        public const string StartSlotName = "Start";

        public override string Title => $"{FunctionName} Start";

        public string FunctionName { get; set; }

        public override Slot OutputExecution => FindSlot(SlotDirection.Output, SlotKind.Execution, SlotFlags.AutoflowExecution);

        public override void RegenerateSlots(IList<Slot> newSlots)
        {
            newSlots.Add(new Slot(SlotDirection.Output, SlotKind.Execution, StartSlotName, SlotFlags.AutoflowExecution));
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