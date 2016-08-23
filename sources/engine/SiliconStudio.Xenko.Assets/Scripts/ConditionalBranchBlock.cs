using Microsoft.CodeAnalysis.CSharp;
using SiliconStudio.Core;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class ConditionalBranchBlock : ExecutionBlock
    {
        [DataMemberIgnore]
        public Slot TrueSlot { get; private set; }

        [DataMemberIgnore]
        public Slot FalseSlot { get; private set; }

        [DataMemberIgnore]
        public Slot ConditionSlot { get; private set; }

        public const string TrueSlotName = "True";
        public const string FalseSlotName = "False";

        public override string Title => "Condition";

        public override void RegenerateSlots()
        {
            Slots.Clear();
            Slots.Add(new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Input });
            Slots.Add(ConditionSlot = new Slot { Kind = SlotKind.Value, Direction = SlotDirection.Input, Name = "Condition" });
            Slots.Add(TrueSlot = new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Output, Name = TrueSlotName });
            Slots.Add(FalseSlot = new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Output, Name = FalseSlotName, Flags = SlotFlags.AutoflowExecution });
        }

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            // Generate false then true block (false block will be reached if the previous condition failed), then true block (reached by goto)
            var falseBlock = context.GetOrCreateBasicBlockFromSlot(FalseSlot);
            var trueBlock = context.GetOrCreateBasicBlockFromSlot(TrueSlot);

            // Generate condition
            var condition = context.GenerateExpression(ConditionSlot) ?? LiteralExpression(SyntaxKind.TrueLiteralExpression);

            // if (condition) goto trueBlock;
            if (trueBlock != null)
                context.AddStatement(IfStatement(condition, context.CreateGotoStatement(trueBlock)));

            // Execution continue in false block
            context.CurrentBasicBlock.NextBlock = falseBlock;
        }
    }
}