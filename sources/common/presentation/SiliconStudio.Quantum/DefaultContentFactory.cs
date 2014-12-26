using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This class is an implementation of the <see cref="IContentFactory"/> interface that can construct <see cref="ObjectContent"/>, <see cref="BoxedContent"/>
    /// and <see cref="MemberContent"/> instances.
    /// </summary>
    public class DefaultContentFactory : IContentFactory
    {
        // TODO: Currently hardcoded until editor plugin system is refactored
        public static readonly List<IContentFactory> PluginContentFactories = new List<IContentFactory>();

        /// <inheritdoc/>
        public virtual IContent CreateObjectContent(INodeBuilder nodeBuilder, object obj, ITypeDescriptor descriptor, bool isPrimitive)
        {
            // Check plugins
            foreach (var pluginContentFactory in PluginContentFactories)
            {
                var result = pluginContentFactory.CreateObjectContent(nodeBuilder, obj, descriptor, isPrimitive);
                if (result != null)
                    return result;
            } 
            
            var reference = nodeBuilder.CreateReferenceForNode(descriptor.Type, obj) as ReferenceEnumerable;
            return new ObjectContent(nodeBuilder, obj, descriptor, isPrimitive, reference);
        }

        /// <inheritdoc/>
        public virtual IContent CreateBoxedContent(INodeBuilder nodeBuilder, object structure, ITypeDescriptor descriptor, bool isPrimitive)
        {
            // Check plugins
            foreach (var pluginContentFactory in PluginContentFactories)
            {
                var result = pluginContentFactory.CreateBoxedContent(nodeBuilder, structure, descriptor, isPrimitive);
                if (result != null)
                    return result;
            }

            return new BoxedContent(nodeBuilder, structure, descriptor, isPrimitive);
        }

        /// <inheritdoc/>
        public virtual IContent CreateMemberContent(INodeBuilder nodeBuilder, IContent container, IMemberDescriptor member, bool isPrimitive, object value)
        {
            // Check plugins
            foreach (var pluginContentFactory in PluginContentFactories)
            {
                var result = pluginContentFactory.CreateMemberContent(nodeBuilder, container, member, isPrimitive, value);
                if (result != null)
                    return result;
            }

            var reference = nodeBuilder.CreateReferenceForNode(member.Type, value);
            return new MemberContent(nodeBuilder, container, member, isPrimitive, reference);
        }
    }
}