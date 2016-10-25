using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SiliconStudio.Core;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class PropertySet : ExecutionBlock
    {
        [RegenerateTitle, RegenerateSlots, BlockDropTarget]
        public Property Property { get; set; }

        public override string Title => Property != null ? $"Set {Property.Name}" : "Set";

        [DataMemberIgnore]
        public Slot InputSlot => FindSlot(SlotDirection.Input, SlotKind.Value, null);

        //[DataMemberIgnore]
        //public Slot OutputSlot => FindSlot(SlotDirection.Output, SlotKind.Value, null);

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            if (Property == null)
                return;

            // Evaluate value
            var newValue = context.GenerateExpression(InputSlot);

            // Generate assignment statement
            context.AddStatement(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(Property.Name), newValue)));
        }

        public override void GenerateSlots(IList<Slot> newSlots, SlotGeneratorContext context)
        {
            newSlots.Add(InputExecutionSlotDefinition);
            newSlots.Add(OutputExecutionSlotDefinition);

            newSlots.Add(new Slot(SlotDirection.Input, SlotKind.Value, type: Property?.Type));
        }
    }
}