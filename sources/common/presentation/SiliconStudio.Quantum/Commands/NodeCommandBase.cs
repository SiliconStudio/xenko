using System.Threading.Tasks;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// Base class for node commands.
    /// </summary>
    public abstract class NodeCommandBase : INodeCommand
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract CombineMode CombineMode { get; }

        /// <inheritdoc/>
        public abstract bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor);

        /// <inheritdoc/>
        public abstract Task Execute(IContent content, Index index, object parameter);

        /// <inheritdoc/>
        public virtual void StartCombinedInvoke()
        {
            // Intentionally do nothing
        }

        /// <inheritdoc/>
        public virtual void EndCombinedInvoke()
        {
            // Intentionally do nothing
        }
    }

    /// <summary>
    /// Base class for node commands that are not asynchronous.
    /// </summary>
    public abstract class SyncNodeCommandBase : NodeCommandBase
    {
        /// <inheritdoc/>
        public sealed override Task Execute(IContent content, Index index, object parameter)
        {
            ExecuteSync(content, index, parameter);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Triggers the command synchromously.
        /// </summary>
        /// <param name="content">The content on which to execute the command.</param>
        /// <param name="index">The index in the content on which to execute the command.</param>
        /// <param name="parameter">The parameter of the command.</param>
        /// <remarks>Implementations of this method should not trigger fire-and-forget actions.</remarks>
        protected abstract void ExecuteSync(IContent content, Index index, object parameter);
    }
}
