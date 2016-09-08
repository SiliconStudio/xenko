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

        public bool IsMemberCall { get; set; }

        public override string Title
        {
            get
            {
                // Take up to two qualifiers (class+method)
                var titleStart = MethodName.LastIndexOf('.');
                titleStart = titleStart > 0 ? MethodName.LastIndexOf('.', titleStart - 1) : -1;

                return MethodName.Substring(titleStart + 1);
            }
        }

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            var arguments = new List<SyntaxNodeOrToken>();
            var memberCallProcessed = false;

            var invocationTarget = MethodName;

            for (int index = 0; index < Slots.Count; index++)
            {
                var slot = Slots[index];
                if (slot.Direction == SlotDirection.Input && slot.Kind == SlotKind.Value)
                {
                    var argument = context.GenerateExpression(slot) ?? IdentifierName("unknown");

                    if (IsMemberCall && !memberCallProcessed)
                    {
                        // this parameter (non-static or extension method)
                        memberCallProcessed = true;
                        invocationTarget = argument.ToFullString() +  "." + invocationTarget;
                        continue;
                    }

                    if (arguments.Count > 0)
                        arguments.Add(Token(SyntaxKind.CommaToken));

                    arguments.Add(Argument(argument));
                }
            }
            context.AddStatement(ExpressionStatement(InvocationExpression(ParseExpression(invocationTarget), ArgumentList(SeparatedList<ArgumentSyntax>(arguments)))));
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