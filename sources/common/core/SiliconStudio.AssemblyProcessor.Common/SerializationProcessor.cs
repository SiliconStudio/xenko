using System;
using System.Collections.Generic;

namespace SiliconStudio.AssemblyProcessor
{
    internal class SerializationProcessor : IAssemblyDefinitionProcessor
    {
        private Action<string> sourceCodeRegisterAction;

        public SerializationProcessor(Action<string> sourceCodeRegisterAction)
        {
            this.sourceCodeRegisterAction = sourceCodeRegisterAction;
        }

        public bool Process(AssemblyProcessorContext context)
        {
            var serializerSourceCode = ComplexSerializerGenerator.GenerateSerializationAssembly(context.AssemblyResolver, context.Assembly, context.Log);
            sourceCodeRegisterAction(serializerSourceCode);

            return true;
        }
    }
}