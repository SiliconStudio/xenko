using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class ConditionalBranchBlock : ExecutionBlock
    {
        private Slot trueSlot;
        private Slot falseSlot;

        public const string TrueSlotName = "True";
        public const string FalseSlotName = "False";

        public override string Title => "Condition";

        public override void RegenerateSlots()
        {
            Slots.Clear();
            Slots.Add(new Slot { Type = SlotType.Execution, Direction = SlotDirection.Input });
            Slots.Add(new Slot { Type = SlotType.Value, Direction = SlotDirection.Input, Name = "Condition" });
            Slots.Add(trueSlot = new Slot { Type = SlotType.Execution, Direction = SlotDirection.Output, Name = TrueSlotName });
            Slots.Add(falseSlot = new Slot { Type = SlotType.Execution, Direction = SlotDirection.Output, Name = FalseSlotName, Flags = SlotFlags.AutoflowExecution });
        }

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            // Generate false then true block (false block will be reached if the previous condition failed), then true block (reached by goto)
            var falseBlock = context.GetOrCreateBasicBlockFromSlot(falseSlot);
            var trueBlock = context.GetOrCreateBasicBlockFromSlot(trueSlot);

            // if (condition) goto trueBlock;
            context.AddStatement(IfStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression), context.CreateGotoStatement(trueBlock)));

            // Execution continue in false block
            context.CurrentBasicBlock.NextBlock = falseBlock;
        }
    }
}