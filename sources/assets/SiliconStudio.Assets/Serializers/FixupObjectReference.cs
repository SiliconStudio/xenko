using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A static class that can be used to fix up object references.
    /// </summary>
    public static class FixupObjectReference
    {
        /// <summary>
        /// Fix up references represented by the <paramref name="objectReferences"/> dictionary into the <paramref name="root"/> object, by visiting the object
        /// to find all <see cref="IIdentifiable"/> instances it references, and modify the references described by <paramref name="objectReferences"/> to point
        /// to the proper identifiable object matching the same <see cref="Guid"/>.
        /// </summary>
        /// <param name="root">The root object to fix up.</param>
        /// <param name="objectReferences">The path to each object reference and the <see cref="Guid"/> of the tar</param>
        /// <param name="throwOnDuplicateIds">If true, an exception will be thrown if two <see cref="IIdentifiable"/></param>
        /// <param name="logger">An optional logger.</param>
        public static void RunFixupPass(object root, Dictionary<YamlAssetPath, Guid> objectReferences, bool throwOnDuplicateIds, [CanBeNull] ILogger logger = null)
        {
            // First collect IIdentifiable objects
            var visitor = new FixupObjectReferenceVisitor(throwOnDuplicateIds, logger);
            visitor.Visit(root);

            // Then resolve and update object references
            foreach (var objectReference in objectReferences)
            {
                IIdentifiable target;
                if (!visitor.ReferenceableObjects.TryGetValue(objectReference.Value, out target))
                {
                    logger?.Warning($"Unable to resolve target object [{objectReference.Value}] of reference [{objectReference.Key}]");
                    continue;
                }
                var path = objectReference.Key.ToMemberPath(root);
                path.Apply(root, MemberPathAction.ValueSet, target);
            }
        }

        private class FixupObjectReferenceVisitor : DataVisitorBase
        {
            public readonly Dictionary<Guid, IIdentifiable> ReferenceableObjects = new Dictionary<Guid, IIdentifiable>();
            private readonly bool throwOnDuplicateIds;
            private readonly ILogger logger;

            public FixupObjectReferenceVisitor(bool throwOnDuplicateIds, [CanBeNull] ILogger logger = null)
            {
                this.throwOnDuplicateIds = throwOnDuplicateIds;
                this.logger = logger;
            }

            public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
            {
                var identifiable = obj as IIdentifiable;
                if (obj is IIdentifiable)
                {
                    if (ReferenceableObjects.ContainsKey(identifiable.Id))
                    {
                        var message = $"Multiple identifiable objects with the same id {identifiable.Id}";
                        logger?.Error(message);
                        if (throwOnDuplicateIds)
                            throw new InvalidOperationException(message);
                    }
                    ReferenceableObjects.Add(identifiable.Id, identifiable);
                }
                base.VisitObject(obj, descriptor, visitMembers);
            }
        }
    }
}
