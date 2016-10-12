using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    public class CustomCodeBlock : ExecutionBlock
    {
        public string Name { get; set; }

        [RegenerateSlots, RegenerateTitle, ScriptCodeAttribute]
        public string Code { get; set; }

        public override string Title => !string.IsNullOrEmpty(Name) ? Name : (!string.IsNullOrEmpty(Code) ? Code : "Custom Code");

        public override void GenerateCode(VisualScriptCompilerContext context)
        {
            var statement = SyntaxFactory.ParseStatement(Code);

            // Forward diagnostics to log
            foreach (var diagnostic in statement.GetDiagnostics())
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

            context.AddStatement(statement);
        }

        public override void GenerateSlots(IList<Slot> newSlots, SlotGeneratorContext context)
        {
            newSlots.Add(InputExecutionSlotDefinition);
            newSlots.Add(OutputExecutionSlotDefinition);

            if (context.Compilation != null && !string.IsNullOrEmpty(Code))
            {
                var statement = SyntaxFactory.ParseStatement($"{{ {Code} }}");

                var block = statement as BlockSyntax;
                if (block != null)
                {
                    RoslynHelper.AnalyzeBlockFlow(newSlots, context.Compilation, block);
                }
            }
        }
    }
}