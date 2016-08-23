using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class CustomCodeBlock : ExecutionBlock
    {
        public string Code { get; set; }

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            var syntaxTree = SyntaxFactory.ParseStatement(Code);

            // Forward diagnostics to log
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
            {
                LogMessageType logType;
                switch (diagnostic.Severity)
                {
                    case DiagnosticSeverity.Info:
                        logType = LogMessageType.Info;
                        break;
                    case DiagnosticSeverity.Warning:
                        logType = LogMessageType.Warning;
                        break;
                    case DiagnosticSeverity.Error:
                        logType = LogMessageType.Error;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                context.Log.Log(new LogMessage(nameof(CustomCodeBlock), logType, diagnostic.GetMessage()));
            }

            context.AddStatement(syntaxTree);
        }

        public override void RegenerateSlots()
        {
            Slots.Clear();
            Slots.Add(new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Input });
            Slots.Add(new Slot { Kind = SlotKind.Execution, Direction = SlotDirection.Output });
        }
    }
}