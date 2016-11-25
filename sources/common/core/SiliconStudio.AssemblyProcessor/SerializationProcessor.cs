using System;
using System.Collections.Generic;

namespace SiliconStudio.AssemblyProcessor
{
    internal class SerializationProcessor : IAssemblyDefinitionProcessor
    {
        public delegate void RegisterSourceCode(string code, string name = null);
        private RegisterSourceCode sourceCodeRegisterAction;

        public SerializationProcessor(RegisterSourceCode sourceCodeRegisterAction)
        {
            this.sourceCodeRegisterAction = sourceCodeRegisterAction;
        }

        public bool Process(AssemblyProcessorContext context)
        {
            var serializerSourceCode = ComplexSerializerGenerator.GenerateSerializationAssembly(context.AssemblyResolver, context.Assembly, context.Log);
            sourceCodeRegisterAction(serializerSourceCode, "DataSerializers");

            return true;
        }
    }
}