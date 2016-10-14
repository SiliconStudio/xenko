using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SiliconStudio.Core;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class VariableSet : ExecutionBlock
    {
        [RegenerateTitle, RegenerateSlots]
        public Variable Variable { get; set; }

        public override string Title => Variable != null ? $"Set {Variable.Name}" : "Set";

        [DataMemberIgnore]
        public Slot InputSlot => FindSlot(SlotDirection.Input, SlotKind.Value, null);

        //[DataMemberIgnore]
        //public Slot OutputSlot => FindSlot(SlotDirection.Output, SlotKind.Value, null);

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            if (Variable == null)
                return;

            // Evaluate value
            var newValue = context.GenerateExpression(InputSlot);

            // Generate assignment statement
            context.AddStatement(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(Variable.Name), newValue)));
        }

        public override void GenerateSlots(IList<Slot> newSlots, SlotGeneratorContext context)
        {
            newSlots.Add(InputExecutionSlotDefinition);
            newSlots.Add(OutputExecutionSlotDefinition);

            newSlots.Add(new Slot(SlotDirection.Input, SlotKind.Value, type: Variable?.Type));
        }
    }
}