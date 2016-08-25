using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SiliconStudio.Core;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class VariableGet : ExpressionBlock
    {
        public Variable Variable { get; set; }

        [DataMemberIgnore]
        public Slot ValueSlot => FindSlot(SlotDirection.Output, SlotKind.Value, null);

        public override ExpressionSyntax GenerateExpression()
        {
            if (Variable == null)
                return null;

            return IdentifierName(Variable.Name);
        }

        public override void RegenerateSlots(IList<Slot> newSlots)
        {
            if (Variable != null)
                newSlots.Add(new Slot { Kind = SlotKind.Value, Direction = SlotDirection.Output, Type = Variable.Type });
        }
    }
}