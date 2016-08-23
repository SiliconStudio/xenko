using System;
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
        public Slot InputSlot { get; set; }

        //[DataMemberIgnore]
        //public Slot OutputSlot { get; set; }

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            if (Variable == null)
                return;

            // Evaluate value
            var newValue = context.GenerateExpression(InputSlot) ?? ConvertLiteralExpression(Variable.Type, InputSlot.Value);

            // Generate assignment statement
            context.AddStatement(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(Variable.Name), newValue)));
        }

        public override void RegenerateSlots()
        {
            Slots.Clear();
            Slots.Add(new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Input });
            Slots.Add(new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Output, Flags = SlotFlags.AutoflowExecution });
            
            // TODO: InputSlot should expose variable type
            Slots.Add(InputSlot = new Slot { Kind = SlotKind.Value, Direction = SlotDirection.Input });
            //Slots.Add(OutputSlot = new Slot { Kind = SlotKind.Value, Direction = SlotDirection.Output });
        }

        private LiteralExpressionSyntax ConvertLiteralExpression(Type type, object value)
        {
            if (type == typeof(bool))
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