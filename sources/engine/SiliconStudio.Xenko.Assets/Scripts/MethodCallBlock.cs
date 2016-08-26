using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class MethodCallBlock : ExecutionBlock
    {
        public string MethodName { get; set; }

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            var arguments = new List<SyntaxNodeOrToken>();
            foreach (var slot in Slots)
            {
                if (slot.Direction == SlotDirection.Input && slot.Kind == SlotKind.Value)
                {
                    if (arguments.Count > 0)
                        arguments.Add(Token(SyntaxKind.CommaToken));

                    var argument = context.GenerateExpression(slot) ?? IdentifierName("unknown");
                    arguments.Add(argument);
                }
            }
            context.AddStatement(ExpressionStatement(InvocationExpression(ParseExpression(MethodName), ArgumentList(SeparatedList<ArgumentSyntax>(arguments)))));
        }

        public override void RegenerateSlots(IList<Slot> newSlots)
        {
            newSlots.Add(InputExecutionSlotDefinition);
            newSlots.Add(OutputExecutionSlotDefinition);

            // Keep old slots
            // We regenerate them based on user actions
            for (int index = 2; index < Slots.Count; index++)
            {
                newSlots.Add(Slots[index]);
            }
        }
    }
}