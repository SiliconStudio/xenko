using System.Text;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    // Used internally by Effect and EffectParameterUpdaterDefinition
    struct ParameterDependency
    {
        public ParameterKey Destination;
        public ParameterKey[] Sources;
        public ParameterDynamicValue Dynamic;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder
                .Append("(")
                .Append(Destination.Name)
                .Append(")");

            foreach (var source in Sources)
            {
                builder
                    .Append(" ")
                    .Append(source.Name);
            }

            return builder.ToString();
        }
    }
}