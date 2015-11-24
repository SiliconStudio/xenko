using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    class FixupValueTypeVisitor : CecilTypeReferenceVisitor
    {
        public static readonly FixupValueTypeVisitor Default = new FixupValueTypeVisitor();

        public override TypeReference Visit(TypeReference type)
        {
            var typeDefinition = type.Resolve();
            if (typeDefinition.IsValueType && !type.IsValueType)
                type.IsValueType = typeDefinition.IsValueType;

            return base.Visit(type);
        }
    }
}