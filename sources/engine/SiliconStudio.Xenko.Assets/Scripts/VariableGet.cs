using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SiliconStudio.Core;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class VariableGet : ExpressionBlock
    {
        [RegenerateTitle, BlockDropTarget]
        public Variable Variable { get; set; }

        public override string Title => Variable != null ? $"Get {Variable.Name}" : "Get";

        [DataMemberIgnore]
        public Slot ValueSlot => FindSlot(SlotDirection.Output, SlotKind.Value, null);

        public override ExpressionSyntax GenerateExpression(VisualScriptCompilerContext context)
        {
            if (Variable == null)
                return IdentifierName("variable_not_set");

            return IdentifierName(Variable.Name);
        }

        public override void GenerateSlots(IList<Slot> newSlots, SlotGeneratorContext context)
        {
            newSlots.Add(new Slot(SlotDirection.Output, SlotKind.Value));
        }
    }
}