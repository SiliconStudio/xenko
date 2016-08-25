using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SiliconStudio.Core;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class VariableSet : ExecutionBlock
    {
        public Variable Variable { get; set; }

        [DataMemberIgnore]
        public Slot InputSlot => FindSlot(SlotDirection.Input, SlotKind.Value, null);

        //[DataMemberIgnore]
        //public Slot OutputSlot => FindSlot(SlotDirection.Output, SlotKind.Value, null);

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            if (Variable == null)
                return;

            // Evaluate value
            var newValue = context.GenerateExpression(InputSlot) ?? ConvertLiteralExpression(Variable.Type, InputSlot.Value);

            // Generate assignment statement
            context.AddStatement(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(Variable.Name), newValue)));
        }

        public override void RegenerateSlots(IList<Slot> newSlots)
        {
            newSlots.Add(InputExecutionSlotDefinition);
            newSlots.Add(OutputExecutionSlotDefinition);

            if (Variable != null)
            {
                newSlots.Add(new Slot { Kind = SlotKind.Value, Direction = SlotDirection.Input, Type = Variable.Type });
                //newSlots.Add(new Slot { Kind = SlotKind.Value, Direction = SlotDirection.Output, Type = Variable.Type });
            }
        }

        private LiteralExpressionSyntax ConvertLiteralExpression(string type, object value)
        {
            if (type == "bool")
            {
                return LiteralExpression(value is bool && (bool)value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
            }
            else
            {
                // TODO: Support more types
                throw new NotImplementedException($"Can't convert literal expression of type {type}");
            }
        }
    }
}