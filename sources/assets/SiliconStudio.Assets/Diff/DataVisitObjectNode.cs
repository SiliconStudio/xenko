using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// The root node used for storing a hierarchy of <see cref="DataVisitNode"/>
    /// </summary>
    public sealed class DataVisitObjectNode : DataVisitNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitObjectNode" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="instanceDescriptor">The instance descriptor.</param>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// instanceDescriptor
        /// </exception>
        public DataVisitObjectNode(object instance, ObjectDescriptor instanceDescriptor) : base(instance, instanceDescriptor)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}", InstanceDescriptor.Type);
        }
    }
}