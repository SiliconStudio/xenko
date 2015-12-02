using System.Linq;
using SiliconStudio.AssemblyProcessor.Serializers;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Collects DataContractAttribute.Alias so that they are registered during Module initialization.
    /// </summary>
    internal class DataContractAliasProcessor : ICecilSerializerProcessor
    {
        public void ProcessSerializers(CecilSerializerContext context)
        {
            foreach (var type in context.Assembly.EnumerateTypes())
            {
                var dataContractAttribute = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "SiliconStudio.Core.DataContractAttribute" || x.AttributeType.FullName == "SiliconStudio.Core.DataAliasAttribute");

                // Only process if ctor with 1 argument
                if (dataContractAttribute == null || !dataContractAttribute.HasConstructorArguments || dataContractAttribute.ConstructorArguments.Count != 1)
                    continue;

                var alias = (string)dataContractAttribute.ConstructorArguments[0].Value;

                context.DataContractAliases.Add(alias, type);
            }
        }
    }
}