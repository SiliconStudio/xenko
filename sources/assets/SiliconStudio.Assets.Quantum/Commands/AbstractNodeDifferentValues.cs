namespace SiliconStudio.Assets.Quantum.Commands
{
    /// <summary>
    /// Represents heterogeneous values.
    /// </summary>
    public class AbstractNodeDifferentValues : AbstractNodeEntry
    {
        /// <summary>
        /// An object that can be passed as parameter to the command, in order to set the value of the node to <c>null</c>.
        /// </summary>
        public static AbstractNodeDifferentValues Default { get; } = new AbstractNodeDifferentValues();

        /// <inheritdoc/>
        public override string DisplayValue => "Different Values";

        /// <inheritdoc/>
        public override object GenerateValue(object currentValue) => currentValue;

        /// <inheritdoc/>
        public override bool IsMatchingValue(object value) => true;
    }
}