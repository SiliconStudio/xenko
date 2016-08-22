using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class VariableGet : ExpressionBlock
    {
        public Variable Variable { get; set; }

        public override ExpressionSyntax GenerateExpression()
        {
            if (Variable == null)
                return null;

            return IdentifierName(Variable.Name);
        }

        public override void RegenerateSlots()
        {
            Slots.Clear();
            Slots.Add(new Slot { Type = SlotType.Value, Direction = SlotDirection.Output });
        }
    }
}